using System.ComponentModel.DataAnnotations;

namespace OrchestrationService.Api.Dtos;

/// <summary>
/// Represents the request payload for starting a new workflow instance.
/// It specifies the type of workflow to initiate and provides the necessary input data.
/// </summary>
public class StartWorkflowRequestDto
{
    /// <summary>
    /// Gets or sets the type or identifier of the workflow to be started.
    /// This should match a registered workflow definition name (e.g., "ReportGenerationSaga", "BlockchainSyncSaga").
    /// </summary>
    /// <example>"ReportGenerationSaga"</example>
    [Required]
    public string WorkflowType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input data for the workflow.
    /// The structure of this object depends on the specific <see cref="WorkflowType"/>.
    /// For example, for "ReportGenerationSaga", this would be <see cref="OrchestrationService.Workflows.ReportGeneration.ReportGenerationSagaInput"/>.
    /// For "BlockchainSyncSaga", this would be <see cref="OrchestrationService.Workflows.BlockchainSync.BlockchainSyncSagaInput"/>.
    /// </summary>
    [Required]
    public object? InputData { get; set; }
}