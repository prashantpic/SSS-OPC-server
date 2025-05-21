namespace OrchestrationService.Configuration;

/// <summary>
/// Defines strongly-typed configuration settings for WorkflowCore.
/// These settings are loaded from the "WorkflowEngine" section in `appsettings.json`.
/// </summary>
public class WorkflowEngineSettings
{
    /// <summary>
    /// Gets or sets the interval at which the workflow host polls for runnable workflows.
    /// Format: "00:00:05" (5 seconds).
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the interval at which the workflow host retries workflows with errors.
    /// Format: "00:00:10" (10 seconds).
    /// </summary>
    public TimeSpan ErrorRetryInterval { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets the interval at which the workflow host polls for pending events.
    /// Format: "00:00:05" (5 seconds).
    /// </summary>
    public TimeSpan EventQueuePollInterval { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    /// Gets or sets the maximum number of retries for a step by default.
    /// Specific activities or workflow definitions can override this.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    // Add other WorkflowCore specific settings if needed, for example:
    // public int MaxConcurrentWorkflows { get; set; } = Environment.ProcessorCount;
    // public TimeSpan WorkflowLockTimeout { get; set; } = TimeSpan.FromMinutes(5);
}