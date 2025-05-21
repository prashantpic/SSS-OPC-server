namespace OrchestrationService.Api.Dtos;

/// <summary>
/// DTO for returning the current status and details of a workflow instance.
/// </summary>
public class WorkflowStatusResponseDto
{
    /// <summary>
    /// The unique identifier of the workflow instance.
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// The current status of the workflow instance (e.g., "Runnable", "Complete", "Terminated", "Suspended").
    /// This is typically a string representation of WorkflowCore.Models.WorkflowStatus.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The time when the workflow instance was created.
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// The time when the workflow instance completed, if applicable.
    /// Null if the workflow is not yet complete.
    /// </summary>
    public DateTime? CompleteTime { get; set; }

    /// <summary>
    /// Error message if the workflow instance failed or was terminated due to an error.
    /// Null if no error occurred or if the workflow is still running.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The current persistent data associated with the workflow instance.
    /// This can be cast to the specific workflow's data type (e.g., ReportGenerationSagaData, BlockchainSyncSagaData)
    /// by the caller if the workflow type is known.
    /// The controller may choose to expose only relevant parts of this data or map it to a more specific DTO.
    /// </summary>
    public object? Data { get; set; }
}