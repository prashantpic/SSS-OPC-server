namespace OrchestrationService.Api.Dtos;

/// <summary>
/// Represents the response payload containing the status and details of a workflow instance.
/// </summary>
public class WorkflowStatusResponseDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the workflow instance.
    /// </summary>
    /// <example>"6b0e00a4-7a31-4f95-890e-2d1f34169285"</example>
    public string WorkflowId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the workflow.
    /// Common values include: "Runnable", "Suspended", "Complete", "Terminated", "Compensatable".
    /// (Refer to WorkflowCore.Models.WorkflowStatus for potential enum mapping if stricter typing is preferred)
    /// </summary>
    /// <example>"Runnable"</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name or identifier of the current step the workflow is executing or paused at.
    /// This might be empty if the workflow is just starting or has completed.
    /// </summary>
    /// <example>"InitiateAiAnalysisActivity"</example>
    public string? CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the last significant event or update for this workflow instance.
    /// </summary>
    public DateTime LastEventTime { get; set; }

    /// <summary>
    /// Gets or sets the persisted data associated with the workflow instance (e.g., ReportGenerationSagaData, BlockchainSyncSagaData).
    /// The actual type of this object will depend on the workflow.
    /// </summary>
    public object? WorkflowData { get; set; }

    /// <summary>
    /// Gets or sets the reason for failure, if the workflow instance has failed.
    /// This will be null if the workflow is not in a failed state.
    /// </summary>
    /// <example>"AI Service returned an error during analysis."</example>
    public string? FailureReason { get; set; }
}