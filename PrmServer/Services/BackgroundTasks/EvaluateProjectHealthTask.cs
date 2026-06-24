using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrmServer.Entities;
using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services.BackgroundTasks
{
    public class EvaluateProjectHealthTask : IScheduledTask
    {
        private readonly ILogger<EvaluateProjectHealthTask> _logger;

        public EvaluateProjectHealthTask(ILogger<EvaluateProjectHealthTask> logger)
        {
            _logger = logger;
        }

        public string TaskName => "EvaluateProjectHealth";

        public async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var projectRepository   = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
            var projectService      = scope.ServiceProvider.GetRequiredService<IProjectService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var emailService        = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var aiService           = scope.ServiceProvider.GetRequiredService<IAiService>();
            var db                  = scope.ServiceProvider.GetRequiredService<PrmDbContext>();

            _logger.LogInformation("[{Task}] Evaluating project health for ACTIVE projects.", TaskName);

            var projects = await projectRepository.GetAllAsync();
            var activeProjects = projects.Where(p =>
                string.Equals(p.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase)).ToList();

            int updated = 0;

            foreach (var project in activeProjects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var newHealth = await projectService.GetHealthAsync(project.Id);

                if (string.Equals(project.Health, newHealth, StringComparison.OrdinalIgnoreCase))
                    continue;

                var previousHealth = project.Health;
                project.Health     = newHealth;
                project.UpdatedAt  = DateTime.UtcNow;
                await projectRepository.UpdateAsync(project);
                updated++;

                _logger.LogInformation(
                    "[{Task}] Project '{ProjectName}' (ID {ProjectId}) health changed: {PreviousHealth} → {NewHealth}.",
                    TaskName, project.Name, project.Id, previousHealth, newHealth);

                await NotifyManagerIfDegradedAsync(
                    notificationService, emailService, aiService, db,
                    project, previousHealth, newHealth, cancellationToken);
            }

            _logger.LogInformation(
                "[{Task}] Health evaluation complete. {UpdatedCount}/{TotalCount} project(s) updated.",
                TaskName, updated, activeProjects.Count);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private async Task NotifyManagerIfDegradedAsync(
            INotificationService notificationService,
            IEmailService emailService,
            IAiService aiService,
            PrmDbContext db,
            Project project,
            string previousHealth,
            string newHealth,
            CancellationToken cancellationToken)
        {
            // Only notify when health degrades (GREEN→AMBER, GREEN→RED, or AMBER→RED)
            if (!IsHealthDegraded(previousHealth, newHealth))
                return;

            // ── In-app notification (existing behaviour) ───────────────────────
            var emoji   = newHealth == "RED" ? "🔴" : "🟡";
            var message = $"{emoji} Project '{project.Name}' health has changed from {previousHealth} to {newHealth}. " +
                          "Please review milestones and resource effort.";

            await notificationService.CreateAsync(project.ManagerId, message, "PROJECT_HEALTH");

            // ── Email notification (new) ────────────────────────────────────────
            // Load manager info
            var manager = await db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == project.ManagerId, cancellationToken);

            if (manager == null || string.IsNullOrWhiteSpace(manager.Email))
            {
                _logger.LogWarning("[{Task}] Manager (ID {ManagerId}) not found or has no email. Skipping at-risk email for project '{ProjectName}'.",
                    TaskName, project.ManagerId, project.Name);
                return;
            }

            // Load milestones for the email detail table
            var milestones = await db.Milestones
                .Where(m => m.ProjectId == project.Id)
                .OrderBy(m => m.DueDate)
                .ToListAsync(cancellationToken);

            // ── AI Risk Summary (graceful fallback if unavailable) ─────────────
            string riskSummary;
            try
            {
                var aiResult = await aiService.GetProjectRiskSummaryAsync(project.Id, manager.Id);
                riskSummary = aiResult.Result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[{Task}] AI Risk Summary failed for project {ProjectId}; using fallback.", TaskName, project.Id);
                riskSummary = BuildFallbackRiskText(project, milestones);
            }

            // ── AI Skill Match (graceful fallback if unavailable) ──────────────
            string suggestedHelp;
            try
            {
                var requirement = $"skills needed to recover a {newHealth} health project: {project.Name}";
                var matchResult = await aiService.GetSkillMatchAsync(requirement, project.Id, null, manager.Id);
                suggestedHelp = matchResult.Recommendations.Any()
                    ? string.Join("", matchResult.Recommendations.Take(5).Select(r =>
                        $"<tr><td style='padding:8px;border:1px solid #e2e8f0;'>{r.FullName}</td>" +
                        $"<td style='padding:8px;border:1px solid #e2e8f0;'>{r.SkillsMatch}</td>" +
                        $"<td style='padding:8px;border:1px solid #e2e8f0;'>{r.Availability}</td></tr>"))
                    : "<tr><td colspan='3' style='padding:8px;color:#6b7280;'>No skill-matched suggestions available.</td></tr>";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[{Task}] AI Skill Match failed for project {ProjectId}; using fallback.", TaskName, project.Id);
                suggestedHelp = "<tr><td colspan='3' style='padding:8px;color:#6b7280;'>Skill match unavailable — AI provider not configured.</td></tr>";
            }

            var subject  = $"[{newHealth}] Project at Risk: {project.Name}";
            var htmlBody = BuildAtRiskEmailHtml(
                manager.FullName, project, milestones, previousHealth, newHealth,
                riskSummary, suggestedHelp);

            await emailService.SendAsync(manager.Email, manager.FullName, subject, htmlBody);

            _logger.LogInformation("[{Task}] At-risk email sent to manager {ManagerEmail} for project '{ProjectName}'.",
                TaskName, manager.Email, project.Name);
        }

        private static string BuildAtRiskEmailHtml(
            string managerName,
            Project project,
            List<Milestone> milestones,
            string previousHealth,
            string newHealth,
            string riskSummary,
            string suggestedHelpRows)
        {
            string healthColor = newHealth switch
            {
                "RED"   => "#ef4444",
                "AMBER" => "#f59e0b",
                _       => "#22c55e"
            };
            string healthEmoji = newHealth switch { "RED" => "🔴", "AMBER" => "🟡", _ => "🟢" };

            var milestoneRows = milestones.Any()
                ? string.Join("", milestones.Select(m =>
                    $"<tr>" +
                    $"<td style='padding:8px;border:1px solid #e2e8f0;'>{m.Title}</td>" +
                    $"<td style='padding:8px;border:1px solid #e2e8f0;'>{m.DueDate:dd-MMM-yyyy}</td>" +
                    $"<td style='padding:8px;border:1px solid #e2e8f0;'>{m.Status}</td>" +
                    $"</tr>"))
                : "<tr><td colspan='3' style='padding:8px;color:#6b7280;'>No milestones configured.</td></tr>";

            return $@"
<!DOCTYPE html><html><body style='font-family:Arial,sans-serif;color:#333;max-width:680px;margin:auto;'>
  <div style='background:{healthColor};padding:20px;border-radius:8px 8px 0 0;'>
    <h2 style='color:#fff;margin:0;'>{healthEmoji} Project Health Alert: {project.Name}</h2>
  </div>
  <div style='padding:24px;border:1px solid #e5e7eb;border-top:none;border-radius:0 0 8px 8px;'>
    <p>Hi <strong>{managerName}</strong>,</p>
    <p>The automated health check has detected a change in your project's status:</p>

    <!-- Project Details -->
    <h3 style='color:#1e293b;border-bottom:2px solid #e5e7eb;padding-bottom:8px;'>📋 Project Details</h3>
    <table style='width:100%;border-collapse:collapse;margin-bottom:24px;'>
      <tr style='background:#f8fafc;'>
        <td style='padding:10px;border:1px solid #e2e8f0;font-weight:bold;width:35%;'>Project Name</td>
        <td style='padding:10px;border:1px solid #e2e8f0;'>{project.Name}</td>
      </tr>
      <tr>
        <td style='padding:10px;border:1px solid #e2e8f0;font-weight:bold;'>Timeline</td>
        <td style='padding:10px;border:1px solid #e2e8f0;'>{project.StartDate:dd-MMM-yyyy} → {project.EndDate:dd-MMM-yyyy}</td>
      </tr>
      <tr style='background:#f8fafc;'>
        <td style='padding:10px;border:1px solid #e2e8f0;font-weight:bold;'>Health Change</td>
        <td style='padding:10px;border:1px solid #e2e8f0;'><strong>{previousHealth} → <span style='color:{healthColor};'>{newHealth}</span></strong></td>
      </tr>
    </table>

    <!-- Health Status Badge -->
    <h3 style='color:#1e293b;border-bottom:2px solid #e5e7eb;padding-bottom:8px;'>🏥 Current Health Status</h3>
    <div style='display:inline-block;background:{healthColor};color:#fff;font-size:18px;font-weight:bold;
                padding:10px 24px;border-radius:24px;margin-bottom:24px;'>
      {healthEmoji} {newHealth}
    </div>

    <!-- Milestones -->
    <h3 style='color:#1e293b;border-bottom:2px solid #e5e7eb;padding-bottom:8px;'>📅 Key Milestones</h3>
    <table style='width:100%;border-collapse:collapse;margin-bottom:24px;'>
      <thead>
        <tr style='background:#1e293b;color:#fff;'>
          <th style='padding:10px;text-align:left;border:1px solid #334155;'>Milestone</th>
          <th style='padding:10px;text-align:left;border:1px solid #334155;'>Due Date</th>
          <th style='padding:10px;text-align:left;border:1px solid #334155;'>Status</th>
        </tr>
      </thead>
      <tbody>{milestoneRows}</tbody>
    </table>

    <!-- AI Risk Summary -->
    <h3 style='color:#1e293b;border-bottom:2px solid #e5e7eb;padding-bottom:8px;'>🤖 AI Risk Summary</h3>
    <div style='background:#fef3c7;border-left:4px solid #f59e0b;padding:16px;border-radius:4px;margin-bottom:24px;
                white-space:pre-wrap;font-size:14px;line-height:1.6;'>{riskSummary}</div>

    <!-- Suggested Help -->
    <h3 style='color:#1e293b;border-bottom:2px solid #e5e7eb;padding-bottom:8px;'>👥 Suggested Help</h3>
    <p style='font-size:13px;color:#6b7280;'>Engineers whose skills may help reduce the project risk:</p>
    <table style='width:100%;border-collapse:collapse;margin-bottom:24px;'>
      <thead>
        <tr style='background:#1e293b;color:#fff;'>
          <th style='padding:10px;text-align:left;border:1px solid #334155;'>Name</th>
          <th style='padding:10px;text-align:left;border:1px solid #334155;'>Matching Skills</th>
          <th style='padding:10px;text-align:left;border:1px solid #334155;'>Availability</th>
        </tr>
      </thead>
      <tbody>{suggestedHelpRows}</tbody>
    </table>

    <p>Please log into the PRM Tool to review milestones and take appropriate action.</p>

    <hr style='border:none;border-top:1px solid #e5e7eb;margin:24px 0;'/>
    <p style='color:#9ca3af;font-size:12px;'>PRM Tool — Automated Project Health Alert</p>
  </div>
</body></html>";
        }

        private static string BuildFallbackRiskText(Project project, List<Milestone> milestones)
        {
            var overdue = milestones.Count(m => m.Status != "COMPLETED" && m.DueDate.Date < DateTime.UtcNow.Date);
            var lines = new List<string>
            {
                $"⚠️ AI provider unavailable. System-generated summary.",
                $"Project: {project.Name}  |  Health: {project.Health}",
                $"Timeline: {project.StartDate:yyyy-MM-dd} → {project.EndDate:yyyy-MM-dd}",
                string.Empty
            };
            lines.Add(overdue > 0
                ? $"🔴 {overdue} overdue milestone(s) detected. Immediate review required."
                : "No overdue milestones at this time.");
            return string.Join("\n", lines);
        }

        private static bool IsHealthDegraded(string previous, string current)
        {
            int Rank(string h) => h?.ToUpperInvariant() switch
            {
                "GREEN" => 0,
                "AMBER" => 1,
                "RED"   => 2,
                _       => -1
            };

            return Rank(current) > Rank(previous);
        }
    }
}
