using PrmServer.Repositories.Interfaces;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services.BackgroundTasks
{
    public class SystemSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISystemConfigService _config;
        private readonly ILogger<SystemSchedulerService> _logger;

        public SystemSchedulerService(
            IServiceScopeFactory scopeFactory,
            ISystemConfigService config,
            ILogger<SystemSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SystemSchedulerService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var intervalMinutes = GetIntervalMinutes();
                _logger.LogInformation(
                    "Scheduler running. Next cycle in {Minutes} minute(s).", intervalMinutes);

                try
                {
                    await RunCycleAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during scheduler cycle.");
                }

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }

            _logger.LogInformation("SystemSchedulerService stopped.");
        }

        private async Task RunCycleAsync()
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var allocationService = scope.ServiceProvider.GetRequiredService<IAllocationService>();
            await allocationService.RecomputeAllEmployeeStatusesAsync();

            // Update project health indicators
            var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
            var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();

            var projects = await projectRepository.GetAllAsync();
            foreach (var project in projects)
            {
                var health = await projectService.GetHealthAsync(project.Id);
                if (project.Health != health)
                {
                    project.Health = health;
                    await projectRepository.UpdateAsync(project);
                }
            }

            // Mark missed timesheets for the previous week
            var timesheetService = scope.ServiceProvider.GetRequiredService<ITimesheetService>();
            await timesheetService.MarkMissedTimesheetsAsync();
        }

        private int GetIntervalMinutes()
        {
            try
            {
                var raw = _config.Get("Scheduler:IntervalMinutes");
                if (int.TryParse(raw, out int parsed) && parsed > 0)
                    return parsed;
            }
            catch
            {
                // Fall through to default
            }

            return 60; // default: every 60 minutes
        }
    }
}
