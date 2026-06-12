using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PrmServer.Services.Interfaces;

namespace PrmServer.Services.BackgroundTasks
{
    public class RecomputeUtilizationTask : IScheduledTask
    {
        private readonly ILogger<RecomputeUtilizationTask> _logger;

        public RecomputeUtilizationTask(ILogger<RecomputeUtilizationTask> logger)
        {
            _logger = logger;
        }

        public string TaskName => "RecomputeUtilization";

        public async Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken)
        {
            var allocationService = scope.ServiceProvider.GetRequiredService<IAllocationService>();

            _logger.LogInformation("[{Task}] Recomputing employee utilization and statuses.", TaskName);

            await allocationService.RecomputeAllEmployeeStatusesAsync();

            _logger.LogInformation("[{Task}] Employee statuses updated.", TaskName);
        }
    }
}
