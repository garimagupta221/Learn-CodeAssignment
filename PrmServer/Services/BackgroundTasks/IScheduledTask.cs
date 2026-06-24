namespace PrmServer.Services.BackgroundTasks
{
    /// <summary>
    /// Strategy interface for individual scheduled background tasks.
    /// Each implementation encapsulates a single scheduling concern (SRP + Strategy Pattern).
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>Human-readable name used in log output.</summary>
        string TaskName { get; }

        /// <summary>Executes the task within a dedicated DI scope.</summary>
        Task ExecuteAsync(IServiceScope scope, CancellationToken cancellationToken);
    }
}
