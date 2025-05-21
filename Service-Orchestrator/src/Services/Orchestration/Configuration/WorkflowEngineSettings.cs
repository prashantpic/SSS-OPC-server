namespace OrchestrationService.Configuration;

/// <summary>
/// Defines strongly-typed configuration settings for WorkflowCore or a similar workflow engine.
/// These settings are typically loaded from the `appsettings.json` file.
/// </summary>
public class WorkflowEngineSettings
{
    /// <summary>
    /// Gets or sets the interval in seconds at which the workflow engine polls for new work
    /// or checks for runnable workflows.
    /// Default value is 5 seconds.
    /// </summary>
    public int PollingIntervalInSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the default interval in seconds to wait before retrying a failed step
    /// in a workflow. This can be overridden by specific retry policies defined in workflows.
    /// Default value is 60 seconds.
    /// </summary>
    public int ErrorRetryIntervalInSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the default maximum number of retries for a failed step.
    /// This can be overridden by specific retry policies.
    /// Default value is 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the duration in seconds for which an idle workflow instance is kept in memory
    /// before being offloaded or passivated by the persistence provider.
    /// Specific to some workflow engine configurations.
    /// Default value is 300 seconds (5 minutes).
    /// </summary>
    public int IdleTimeoutSeconds { get; set; } = 300;
}