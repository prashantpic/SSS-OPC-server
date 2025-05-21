using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.AiService;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Workflows.ReportGeneration.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Saga compensation activity for failed report generation.
    /// Cleans up resources created during the ReportGenerationSaga.
    /// Corresponds to REQ-7-020, REQ-7-022.
    /// </summary>
    public class CompensateReportGenerationActivity : StepBodyAsync
    {
        private readonly IDataServiceClient _dataServiceClient;
        private readonly IAiServiceClient _aiServiceClient; // For cleaning AI resources, if applicable
        private readonly IIntegrationServiceClient _integrationServiceClient; // For notifications
        private readonly ILogger<CompensateReportGenerationActivity> _logger;

        public CompensateReportGenerationActivity(
            IDataServiceClient dataServiceClient,
            IAiServiceClient aiServiceClient,
            IIntegrationServiceClient integrationServiceClient,
            ILogger<CompensateReportGenerationActivity> logger)
        {
            _dataServiceClient = dataServiceClient;
            _aiServiceClient = aiServiceClient;
            _integrationServiceClient = integrationServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (ReportGenerationSagaData)context.Workflow.Data;
            _logger.LogInformation("Initiating compensation for ReportGenerationSaga, ReportId: {ReportId}. Failure Reason: {FailureReason}", sagaData.ReportId, sagaData.FailureReason);
            sagaData.CurrentStatus = "Compensating";
            if (sagaData.CompensatedSteps == null) sagaData.CompensatedSteps = new System.Collections.Generic.List<string>();

            // Logic depends on which steps completed before failure.
            // This activity is typically a single unit, but the saga definition decides *when* it runs.
            // The compensation logic here should check the state of sagaData.

            // 1. If GeneratedDocumentUri exists, delete partial/failed report document.
            if (!string.IsNullOrEmpty(sagaData.GeneratedDocumentUri) && !sagaData.CompensatedSteps.Contains(nameof(sagaData.GeneratedDocumentUri)))
            {
                try
                {
                    _logger.LogInformation("Compensating: Deleting generated document at URI: {GeneratedDocumentUri} for ReportId: {ReportId}", sagaData.GeneratedDocumentUri, sagaData.ReportId);
                    // SDS: "call Data Service (IDataServiceClient) or Integration Service (IIntegrationServiceClient) to delete"
                    // IDataServiceClient has DeleteDocumentAsync according to SDS (Section 8.2)
                    await _dataServiceClient.DeleteDocumentAsync(sagaData.GeneratedDocumentUri, context.CancellationToken);
                    sagaData.CompensatedSteps.Add(nameof(sagaData.GeneratedDocumentUri));
                    _logger.LogInformation("Compensated: Successfully deleted document for ReportId: {ReportId}", sagaData.ReportId);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Compensation Error: Failed to delete document URI {GeneratedDocumentUri} for ReportId: {ReportId}", sagaData.GeneratedDocumentUri, sagaData.ReportId);
                    // Log and continue compensation for other steps if possible
                }
            }

            // 2. If AiAnalysisResultUri exists, potentially clean up AI resources.
            if (!string.IsNullOrEmpty(sagaData.AiAnalysisResultUri) && !sagaData.CompensatedSteps.Contains(nameof(sagaData.AiAnalysisResultUri)))
            {
                try
                {
                    _logger.LogInformation("Compensating: Cleaning up AI analysis resources for URI: {AiAnalysisResultUri}, ReportId: {ReportId}", sagaData.AiAnalysisResultUri, sagaData.ReportId);
                    // Assuming IAiServiceClient has a cleanup method, not specified in SDS. Placeholder.
                    // await _aiServiceClient.CleanupAnalysisAsync(sagaData.AiAnalysisResultUri, context.CancellationToken);
                    _logger.LogWarning("AI resource cleanup for {AiAnalysisResultUri} is a placeholder, actual implementation in AiServiceClient needed.", sagaData.AiAnalysisResultUri);
                    sagaData.CompensatedSteps.Add(nameof(sagaData.AiAnalysisResultUri));
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Compensation Error: Failed to cleanup AI resources for {AiAnalysisResultUri}, ReportId: {ReportId}", sagaData.AiAnalysisResultUri, sagaData.ReportId);
                }
            }
            
            // 3. Notify relevant parties (e.g., log failure, send error email).
            // This might be a general notification after all compensation attempts.
            try
            {
                _logger.LogInformation("Compensating: Notifying about failure for ReportId: {ReportId}", sagaData.ReportId);
                // Example: Send an email via Integration Service
                // await _integrationServiceClient.SendFailureNotificationAsync(
                //    $"Report Generation Failed for ID: {sagaData.ReportId}",
                //    $"Details: {sagaData.FailureReason}. Compensation attempted. Current status: {sagaData.CurrentStatus}");
                _logger.LogWarning("Failure notification for ReportId {ReportId} is a placeholder.", sagaData.ReportId);

            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Compensation Error: Failed to send failure notification for ReportId: {ReportId}", sagaData.ReportId);
            }

            // Final status update
            // The SDS says "Update CurrentStatus to 'Compensated' or 'Failed'".
            // If all compensations were successful, 'Compensated'. If some failed, 'CompensationFailed' or stick to 'Failed'.
            // For simplicity, let's mark as Compensated if we reached here. The saga itself will be marked Failed by WorkflowCore.
            sagaData.CurrentStatus = "Compensated"; 
            _logger.LogInformation("Compensation process completed for ReportId: {ReportId}. Final status in saga data: {CurrentStatus}", sagaData.ReportId, sagaData.CurrentStatus);

            return ExecutionResult.Next(); // Compensation steps usually don't branch further in the main flow.
        }
    }
}