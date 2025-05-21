using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrchestrationService.Api.Dtos;

/// <summary>
/// DTO for starting a generic workflow.
/// </summary>
public class StartWorkflowRequestDto
{
    /// <summary>
    /// The name or identifier of the workflow to start (e.g., "ReportGenerationSaga").
    /// This should match the Id property of the IWorkflow implementation.
    /// </summary>
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    /// Optional version of the workflow to start. If not specified, the workflow host
    /// might default to the latest registered version.
    /// </summary>
    public int? Version { get; set; }

    /// <summary>
    /// The input data for the workflow. This should be a JSON object that can be
    /// deserialized into the specific workflow's input type (e.g., ReportGenerationSagaInput, BlockchainSyncSagaInput).
    /// The controller receiving this DTO will handle deserializing this JsonElement to the concrete type.
    /// </summary>
    public JsonElement InputData { get; set; }
}