using System.Threading.Tasks;
using OrchestrationService.Workflows.ReportGeneration.Activities; // For DTOs like AiAnalysisRequestDto, GenerateReportRequestDto

namespace OrchestrationService.Infrastructure.HttpClients.AiService;

/// <summary>
/// Defines the contract for communication with the external AI Processing Service (REPO-SERVER-AI).
/// Abstracts the details of remote invocation for workflow activities.
/// </summary>
public interface IAiServiceClient
{
    /// <summary>
    /// Initiates an AI analysis process.
    /// </summary>
    /// <param name="request">The analysis request parameters.</param>
    /// <returns>A DTO containing the result URI or reference from the AI service.</returns>
    Task<AiAnalysisResultDto> InitiateAnalysisAsync(AiAnalysisRequestDto request);

    /// <summary>
    /// Cleans up resources related to a previously initiated AI analysis, typically used in compensation.
    /// </summary>
    /// <param name="resultUri">The URI or reference of the AI analysis result to clean up.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CleanupAnalysisResultAsync(string resultUri);

    /// <summary>
    /// Requests the AI service to generate a report document based on provided data references and analysis results.
    /// </summary>
    /// <param name="request">The report generation request parameters, including references to historical data and AI analysis.</param>
    /// <returns>A DTO containing the URI or reference of the generated report document.</returns>
    Task<GenerateReportResponseDto> GenerateReportAsync(GenerateReportRequestDto request);
}