using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services.BackgroundTasks
{
    public class MarkMissedTimesheetsTask : IScheduledTask
    {
        private readonly ILogger<MarkMissedTimesheetsTask> _logger;

        public MarkMissedTimesheetsTask(ILogger<MarkMissedTimesheetsTask> logger)
        {
            _logger = logger;
        }

        public string TaskName => "MarkMissedTimesheets";

        public async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var timesheetService    = scope.ServiceProvider.GetRequiredService<ITimesheetService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            _logger.LogInformation("[{Task}] Checking for unsubmitted timesheets from the previous week.", TaskName);

            var missedUserIds = await timesheetService.MarkMissedTimesheetsAsync();

            if (missedUserIds.Count == 0)
            {
                _logger.LogInformation("[{Task}] No missing timesheets found. Nothing to mark.", TaskName);
                return;
            }

            _logger.LogInformation(
                "[{Task}] Marked {Count} timesheet(s) as MISSED. Sending notifications.",
                TaskName, missedUserIds.Count);

            var weekStart = GetPreviousMonday();
            var weekLabel = weekStart.ToString("dd-MMM-yyyy");

            foreach (var userId in missedUserIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var message = $"⚠ Your timesheet for week {weekLabel} has been marked as MISSED. " +
                              "Please submit it as soon as possible.";

                await notificationService.CreateAsync(userId, message, "MISSED_TIMESHEET");
            }

            _logger.LogInformation("[{Task}] Notifications sent to {Count} employee(s).", TaskName, missedUserIds.Count);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static DateTime GetPreviousMonday()
        {
            var today = DateTime.UtcNow.Date;
            int daysSinceMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return today.AddDays(-daysSinceMonday - 7);
        }
    }
}
