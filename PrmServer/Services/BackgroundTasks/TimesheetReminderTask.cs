using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services.BackgroundTasks
{
    /// <summary>
    /// Runs after <see cref="MarkMissedTimesheetsTask"/> each scheduler cycle.
    /// Drives the three-step escalation for employees who have a MISSED timesheet:
    ///
    ///   Step 1  Reminder 1 email (first cycle after miss is detected)
    ///   Step 2  Reminder 2 email (≥1 working day after Reminder 1)
    ///   Step 3  Freeze access + notify employee + notify manager
    ///           (≥1 working day after Reminder 2)
    ///
    /// A <see cref="TimesheetReminderLog"/> row per (UserId, WeekStart) persists state
    /// across scheduler cycles, guaranteeing each step is sent exactly once.
    /// </summary>
    public class TimesheetReminderTask : IScheduledTask
    {
        // Minimum gap between escalation steps (in hours, treating 8 h ≈ one working day)
        private const int WorkingDayHours = 8;

        private readonly ILogger<TimesheetReminderTask> _logger;

        public TimesheetReminderTask(ILogger<TimesheetReminderTask> logger)
        {
            _logger = logger;
        }

        public string TaskName => "TimesheetReminder";

        public async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var db                  = scope.ServiceProvider.GetRequiredService<PrmDbContext>();
            var reminderRepo        = scope.ServiceProvider.GetRequiredService<ITimesheetReminderRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var emailService        = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var previousMonday = GetPreviousMonday();
            var weekLabel      = previousMonday.ToString("dd-MMM-yyyy");

            _logger.LogInformation("[{Task}] Processing reminder escalation for week {Week}.", TaskName, weekLabel);

            // Find all MISSED records for the previous week
            var missedTimesheets = await db.Timesheets
                .Where(t => t.WeekStart.Date == previousMonday && t.Status == "MISSED")
                .Include(t => t.User)
                .ToListAsync(cancellationToken);

            if (!missedTimesheets.Any())
            {
                _logger.LogInformation("[{Task}] No MISSED timesheets found for week {Week}.", TaskName, weekLabel);
                return;
            }

            _logger.LogInformation("[{Task}] Found {Count} MISSED timesheet(s) for week {Week}.",
                TaskName, missedTimesheets.Count, weekLabel);

            var now = DateTime.UtcNow;

            foreach (var ts in missedTimesheets)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var user = ts.User;
                if (user == null)
                    continue;

                var log = await reminderRepo.GetByUserAndWeekAsync(user.Id, previousMonday);

                // ── Step A: Reminder 1 ───────────────────────────────────────────
                if (log == null)
                {
                    log = new TimesheetReminderLog
                    {
                        UserId    = user.Id,
                        WeekStart = previousMonday,
                        Reminder1SentAt = now
                    };
                    await reminderRepo.AddAsync(log);

                    await emailService.SendAsync(
                        user.Email, user.FullName,
                        $"⚠️ Timesheet Reminder: Week of {weekLabel}",
                        BuildReminder1Html(user.FullName, weekLabel));

                    await notificationService.CreateAsync(user.Id,
                        $"⚠️ Reminder: Your timesheet for week {weekLabel} is still missing. Please submit it.",
                        "TIMESHEET_REMINDER_1");

                    _logger.LogInformation("[{Task}] Reminder 1 sent to {User} (ID {Id}).",
                        TaskName, user.FullName, user.Id);
                    continue;
                }

                // ── Step B: Reminder 2 ───────────────────────────────────────────
                if (log.Reminder2SentAt == null &&
                    log.Reminder1SentAt.HasValue &&
                    (now - log.Reminder1SentAt.Value).TotalHours >= WorkingDayHours)
                {
                    log.Reminder2SentAt = now;
                    await reminderRepo.UpdateAsync(log);

                    await emailService.SendAsync(
                        user.Email, user.FullName,
                        $"🔔 Final Timesheet Reminder: Week of {weekLabel}",
                        BuildReminder2Html(user.FullName, weekLabel));

                    await notificationService.CreateAsync(user.Id,
                        $"🔔 Final Reminder: Timesheet for week {weekLabel} is still missing. " +
                        "Continued non-submission will result in access being frozen.",
                        "TIMESHEET_REMINDER_2");

                    _logger.LogInformation("[{Task}] Reminder 2 sent to {User} (ID {Id}).",
                        TaskName, user.FullName, user.Id);
                    continue;
                }

                // ── Step C: Freeze ───────────────────────────────────────────────
                if (!log.IsFrozen &&
                    log.Reminder2SentAt.HasValue &&
                    (now - log.Reminder2SentAt.Value).TotalHours >= WorkingDayHours)
                {
                    // Freeze the employee
                    var dbUser = await db.Users.FindAsync(new object[] { user.Id }, cancellationToken);
                    if (dbUser != null)
                    {
                        dbUser.TimesheetAccessFrozen = true;
                        await db.SaveChangesAsync(cancellationToken);
                    }

                    log.IsFrozen = true;
                    await reminderRepo.UpdateAsync(log);

                    // Notify employee
                    await emailService.SendAsync(
                        user.Email, user.FullName,
                        "🔒 Timesheet Access Frozen",
                        BuildFreezeEmployeeHtml(user.FullName, weekLabel));

                    await notificationService.CreateAsync(user.Id,
                        $"🔒 Your timesheet access has been frozen due to missing submission for week {weekLabel}. " +
                        "Please contact your reporting manager to restore access.",
                        "TIMESHEET_FROZEN");

                    // Notify manager (if set)
                    if (user.ManagerId.HasValue)
                    {
                        var manager = await db.Users.FindAsync(new object[] { user.ManagerId.Value }, cancellationToken);
                        if (manager != null && !string.IsNullOrWhiteSpace(manager.Email))
                        {
                            await emailService.SendAsync(
                                manager.Email, manager.FullName,
                                $"[Action Required] {user.FullName}'s Timesheet Access Has Been Frozen",
                                BuildFreezeManagerHtml(manager.FullName, user.FullName, user.Email, weekLabel));

                            await notificationService.CreateAsync(manager.Id,
                                $"🔒 {user.FullName}'s timesheet access has been frozen for week {weekLabel}. " +
                                "Please review and restore access from the Frozen Timesheets screen.",
                                "TIMESHEET_FROZEN_MANAGER");
                        }
                    }

                    _logger.LogWarning("[{Task}] Timesheet access FROZEN for {User} (ID {Id}) — week {Week}.",
                        TaskName, user.FullName, user.Id, weekLabel);
                }
            }
        }

        // ── Email HTML builders ────────────────────────────────────────────────────

        private static string BuildReminder1Html(string name, string weekLabel) => $@"
<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
  <div style='background:#f59e0b;padding:20px;border-radius:8px 8px 0 0;'>
    <h2 style='color:#fff;margin:0;'>⚠️ Timesheet Reminder</h2>
  </div>
  <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
    <p>Hi <strong>{name}</strong>,</p>
    <p>This is a friendly reminder that your timesheet for the week of <strong>{weekLabel}</strong> has not been submitted yet.</p>
    <p>Please log in to the PRM Tool and submit your timesheet at your earliest convenience.</p>
    <p style='color:#6b7280;font-size:13px;margin-top:32px;'>If you believe this is an error, please contact your reporting manager.</p>
    <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0;'/>
    <p style='color:#9ca3af;font-size:12px;'>PRM Tool — Automated Notification</p>
  </div>
</body></html>";

        private static string BuildReminder2Html(string name, string weekLabel) => $@"
<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
  <div style='background:#ef4444;padding:20px;border-radius:8px 8px 0 0;'>
    <h2 style='color:#fff;margin:0;'>🔔 Final Timesheet Reminder</h2>
  </div>
  <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
    <p>Hi <strong>{name}</strong>,</p>
    <p>Your timesheet for the week of <strong>{weekLabel}</strong> is still missing.</p>
    <p><strong style='color:#ef4444;'>This is your final reminder.</strong> If your timesheet is not submitted soon, your timesheet submission access will be <strong>frozen</strong> and you will need your manager to restore it.</p>
    <p>Please log in to the PRM Tool immediately and submit your timesheet.</p>
    <p style='color:#6b7280;font-size:13px;margin-top:32px;'>If you believe this is an error, please contact your reporting manager.</p>
    <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0;'/>
    <p style='color:#9ca3af;font-size:12px;'>PRM Tool — Automated Notification</p>
  </div>
</body></html>";

        private static string BuildFreezeEmployeeHtml(string name, string weekLabel) => $@"
<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
  <div style='background:#7c3aed;padding:20px;border-radius:8px 8px 0 0;'>
    <h2 style='color:#fff;margin:0;'>🔒 Timesheet Access Frozen</h2>
  </div>
  <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
    <p>Hi <strong>{name}</strong>,</p>
    <p>Because the timesheet for the week of <strong>{weekLabel}</strong> was not submitted after two reminders, your <strong>timesheet submission access has been temporarily frozen</strong>.</p>
    <ul style='line-height:1.8;'>
      <li>You can still <strong>log in</strong> and <strong>view</strong> your timesheets.</li>
      <li>You <strong>cannot create, update, or submit</strong> new timesheet entries until access is restored.</li>
    </ul>
    <p>Please contact your <strong>reporting manager</strong> to have your access reviewed and restored.</p>
    <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0;'/>
    <p style='color:#9ca3af;font-size:12px;'>PRM Tool — Automated Notification</p>
  </div>
</body></html>";

        private static string BuildFreezeManagerHtml(string managerName, string employeeName, string employeeEmail, string weekLabel) => $@"
<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;color:#333;max-width:600px;margin:auto;'>
  <div style='background:#1e293b;padding:20px;border-radius:8px 8px 0 0;'>
    <h2 style='color:#fff;margin:0;'>🔒 [Action Required] Timesheet Access Frozen</h2>
  </div>
  <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
    <p>Hi <strong>{managerName}</strong>,</p>
    <p>The timesheet submission access for one of your direct reports has been frozen after two missed reminders:</p>
    <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
      <tr style='background:#f8fafc;'>
        <td style='padding:10px;border:1px solid #e2e8f0;font-weight:bold;width:40%;'>Employee</td>
        <td style='padding:10px;border:1px solid #e2e8f0;'>{employeeName}</td>
      </tr>
      <tr>
        <td style='padding:10px;border:1px solid #e2e8f0;font-weight:bold;'>Email</td>
        <td style='padding:10px;border:1px solid #e2e8f0;'>{employeeEmail}</td>
      </tr>
      <tr style='background:#f8fafc;'>
        <td style='padding:10px;border:1px solid #e2e8f0;font-weight:bold;'>Missing Week</td>
        <td style='padding:10px;border:1px solid #e2e8f0;'>{weekLabel}</td>
      </tr>
    </table>
    <p>Please review the situation and restore access from the <strong>Frozen Timesheets</strong> screen in the Manager Panel.</p>
    <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0;'/>
    <p style='color:#9ca3af;font-size:12px;'>PRM Tool — Automated Notification</p>
  </div>
</body></html>";

        // ── Helpers ────────────────────────────────────────────────────────────────

        private static DateTime GetPreviousMonday()
        {
            var today = DateTime.UtcNow.Date;
            int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return today.AddDays(-daysSinceMonday - 7);
        }
    }
}
