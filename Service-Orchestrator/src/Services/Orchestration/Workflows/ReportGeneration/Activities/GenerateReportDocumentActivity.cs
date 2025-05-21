using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.AiService;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Workflows.ReportGeneration.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Workflow activity to generate the report document.
    /// Corresponds to REQ-7-020.
    /// </summary>
    public class GenerateReportDocumentActivity : StepBodyAsync
    {
        private readonly IAiServiceClient _aiServiceClient;
        private readonly IIntegrationServiceClient _integrationServiceClient; // If a dedicated reporting service is used via Integration
        private readonly ILogger<GenerateReportDocumentActivity> _logger;

        // Inputs from SagaData (implicitly via context)
        // Outputs to SagaData

        public GenerateReportDocumentActivity(
            IAiServiceClient aiServiceClient,
            IIntegrationServiceClient integrationServiceClient,
            ILogger<GenerateReportDocumentActivity> logger)
        {
            _aiServiceClient = aiServiceClient;
            _integrationServiceClient = integrationServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (ReportGenerationSagaData)context.Workflow.Data;
            _logger.LogInformation("Generating report document for ReportId: {ReportId}", sagaData.ReportId);

            if (sagaData.RequestParameters == null || string.IsNullOrEmpty(sagaData.AiAnalysisResultUri))
            {
                _logger.LogError("Missing required parameters (RequestParameters or AiAnalysisResultUri) for ReportId: {ReportId} to generate document.", sagaData.ReportId);
                sagaData.CurrentStatus = "Failed_DocGeneration_MissingParams";
                sagaData.FailureReason = "Missing required parameters for document generation.";
                return ExecutionResult.Outcome("Error");
            }

            try
            {
                // The SDS implies either AI Service or Integration Service (for a dedicated reporting service)
                // Let's assume IAiServiceClient.GenerateReportDocumentAsync is the primary choice for now
                // The SDS mentions IAiServiceClient.GenerateReportDocumentAsync(ReportGenerationData reportData, ...)
                // or IIntegrationServiceClient.GenerateReportAsync(params)
                // Let's define a placeholder ReportGenerationInputForDocument DTO

                var documentGenerationParams = new ReportGenerationInputForDocument
                {
                    ReportType = sagaData.RequestParameters.ReportType,
                    CustomParameters = sagaData.RequestParameters.Parameters,
                    AiAnalysisResultUri = sagaData.AiAnalysisResultUri,
                    HistoricalDataRef = sagaData.HistoricalDataRef 
                    // Add other necessary fields
                };

                ReportDocumentResult documentResult;
                // This logic might be configurable based on ReportType or a system setting
                bool useAiServiceForGeneration = true; // Example flag, could come from config

                if (useAiServiceForGeneration)
                {
                     documentResult = await _aiServiceClient.GenerateReportDocumentAsync(documentGenerationParams, context.CancellationToken);
                }
                else
                {
                    // Placeholder for using IntegrationServiceClient if it routes to a dedicated reporting service
                    // documentResult = await _integrationServiceClient.GenerateReportAsync(documentGenerationParams, context.CancellationToken);
                    _logger.LogWarning("IntegrationServiceClient.GenerateReportAsync not yet fully implemented as primary. Using AI Service. ReportId: {ReportId}", sagaData.ReportId);
                    documentResult = await _aiServiceClient.GenerateReportDocumentAsync(documentGenerationParams, context.CancellationToken); // Fallback or primary
                }


                if (documentResult == null || string.IsNullOrEmpty(documentResult.DocumentUri))
                {
                    _logger.LogError("Generated document URI is null or empty for ReportId: {ReportId}.", sagaData.ReportId);
                    sagaData.CurrentStatus = "Failed_DocGeneration_NoUri";
                    sagaData.FailureReason = "Report generation service did not return a valid document URI.";
                    return ExecutionResult.Outcome("Error");
                }

                sagaData.GeneratedDocumentUri = documentResult.DocumentUri;
                _logger.LogInformation("Report document generated successfully. URI: {GeneratedDocumentUri} for ReportId: {ReportId}", sagaData.GeneratedDocumentUri, sagaData.ReportId);
                
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating report document for ReportId: {ReportId}. Error: {ErrorMessage}", sagaData.ReportId, ex.Message);
                sagaData.FailureReason = $"Report Document Generation failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore
            }
        }
    }
}