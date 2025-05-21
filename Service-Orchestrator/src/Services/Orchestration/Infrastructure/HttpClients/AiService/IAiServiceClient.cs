using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OrchestrationService.Infrastructure.HttpClients.AiService
{
    // Placeholder DTOs - these would ideally be in a shared models library
    public record AiAnalysisRequestDto(Dictionary<string, object> Parameters);
    public record AiAnalysisResultDto(string ResultUri, string Status);
    public record GenerateDocumentRequestDto(string ReportType, Dictionary<string, object> Parameters, string AiAnalysisResultUri, string HistoricalDataRef);
    public record ReportDocumentDto(string DocumentUri, string Format);

    /// <summary>
    /// Defines the contract for communication with the external AI Processing Service.
    /// Abstracts the details of remote invocation for workflow activities.
    /// Implements REQ-7-020.
    /// </summary>
    public interface IAiServiceClient
    {
        /// <summary>
        /// Initiates an AI analysis process.
        /// </summary>
        /// <param name="parameters">Parameters for the AI analysis.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the AI analysis initiation, typically a reference or URI to the results.</returns>
        Task<AiAnalysisResultDto> InitiateAnalysisAsync(AiAnalysisRequestDto parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a report document using AI capabilities.
        /// </summary>
        /// <param name="reportData">Data required for generating the report document.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The generated report document details, typically a URI to the document.</returns>
        Task<ReportDocumentDto> GenerateReportDocumentAsync(GenerateDocumentRequestDto reportData, CancellationToken cancellationToken = default);
    }
}