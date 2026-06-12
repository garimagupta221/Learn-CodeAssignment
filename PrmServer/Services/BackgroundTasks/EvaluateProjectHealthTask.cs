using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                    notificationService, project.ManagerId, project.Name, previousHealth, newHealth);
            }

            _logger.LogInformation(
                "[{Task}] Health evaluation complete. {UpdatedCount}/{TotalCount} project(s) updated.",
                TaskName, updated, activeProjects.Count);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static async Task NotifyManagerIfDegradedAsync(
            INotificationService notificationService,
            int managerId,
            string projectName,
            string previousHealth,
            string newHealth)
        {
            // Notify only when health degrades (GREEN→AMBER, GREEN→RED, or AMBER→RED)
            bool isDegraded = IsHealthDegraded(previousHealth, newHealth);
            if (!isDegraded)
                return;

            var emoji   = newHealth == "RED" ? "🔴" : "🟡";
            var message = $"{emoji} Project '{projectName}' health has changed from {previousHealth} to {newHealth}. " +
                          "Please review milestones and resource effort.";

            await notificationService.CreateAsync(managerId, message, "PROJECT_HEALTH");
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
