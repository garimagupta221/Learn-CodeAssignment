using PrmServer.Services.Interfaces;

namespace PrmServer.Services.BackgroundTasks
{
    public class SystemSchedulerService : BackgroundService
    {
        private const int StartupDelaySeconds = 30;
        private const int DefaultIntervalHours = 4;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISystemConfigService _config;
        private readonly ILogger<SystemSchedulerService> _logger;

        public SystemSchedulerService(
            IServiceScopeFactory scopeFactory,
            ISystemConfigService config,
            ILogger<SystemSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _config       = config;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "SystemSchedulerService started. Waiting {Delay}s before first cycle.",
                StartupDelaySeconds);

            await Task.Delay(TimeSpan.FromSeconds(StartupDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var intervalHours = ResolveIntervalHours();

                _logger.LogInformation(
                    "=== Scheduler cycle starting. Next run in {Hours:F1} hour(s). ===", intervalHours);

                await RunAllTasksAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }

            _logger.LogInformation("SystemSchedulerService stopped.");
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private async Task RunAllTasksAsync(CancellationToken stoppingToken)
        {
            // Create a single scope shared across all tasks in one cycle
            await using var scope = _scopeFactory.CreateAsyncScope();

            var tasks = scope.ServiceProvider.GetServices<IScheduledTask>().ToList();

            foreach (var task in tasks)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;

                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    _logger.LogInformation("[{Task}] Starting.", task.TaskName);
                    await task.ExecuteAsync(scope, stoppingToken);
                    sw.Stop();
                    _logger.LogInformation(
                        "[{Task}] Completed in {ElapsedMs}ms.", task.TaskName, sw.ElapsedMilliseconds);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("[{Task}] Cancelled by shutdown request.", task.TaskName);
                    break;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError(
                        ex,
                        "[{Task}] Failed after {ElapsedMs}ms. Other tasks will still run.",
                        task.TaskName, sw.ElapsedMilliseconds);
                    // Intentionally continue to next task — one failure must not block others
                }
            }
        }

        /// <summary>
        /// Reads the scheduler interval from persistent config (unit: hours).
        /// Falls back to <see cref="DefaultIntervalHours"/> if the key is missing or invalid.
        /// Re-read on every cycle so Admin changes take effect without a restart.
        /// </summary>
        private double ResolveIntervalHours()
        {
            try
            {
                var raw = _config.Get("SchedulerInterval");
                if (double.TryParse(raw, out double parsed) && parsed > 0)
                    return parsed;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read SchedulerInterval from config. Using default {Default}h.",
                    DefaultIntervalHours);
            }

            return DefaultIntervalHours;
        }
    }
}
