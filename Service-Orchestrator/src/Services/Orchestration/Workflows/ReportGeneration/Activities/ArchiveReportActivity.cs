using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Workflows.ReportGeneration.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Workflow activity for archiving and versioning reports.
    /// Corresponds to REQ-7-020, REQ-7-022.
    /// </summary>
    public class ArchiveReportActivity : StepBodyAsync
    {
        private readonly IDataServiceClient _dataServiceClient;
        private readonly ILogger<ArchiveReportActivity> _logger;

        public ArchiveReportActivity(IDataServiceClient dataServiceClient, ILogger<ArchiveReportActivity> logger)
        {
            _dataServiceClient = dataServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (ReportGenerationSagaData)context.Workflow.Data;
            _logger.LogInformation("Archiving report for ReportId: {ReportId}", sagaData.ReportId);

            if (string.IsNullOrEmpty(sagaData.GeneratedDocumentUri) || string.IsNullOrEmpty(sagaData.ReportId))
            {
                _logger.LogError("Missing GeneratedDocumentUri or ReportId for ReportId: {ReportId}. Cannot archive.", sagaData.ReportId);
                sagaData.CurrentStatus = "Failed_Archiving_MissingParams";
                sagaData.FailureReason = "Missing document URI or report ID for archiving.";
                return ExecutionResult.Outcome("Error");
            }

            try
            {
                // The SDS mentions IDataServiceClient.ArchiveReportAsync(id, uri)
                // The output is `ArchivedReportUri` in SagaData as per SDS, but the client method signature is void.
                // Let's assume the ArchiveReportAsync confirms success, and we can use GeneratedDocumentUri as ArchivedReportUri if no new URI is returned.
                // Or DataService could update a central registry and the URI remains the same but its state is 'Archived'.
                await _dataServiceClient.ArchiveReportAsync(sagaData.ReportId, sagaData.GeneratedDocumentUri, context.CancellationToken);
                
                // As per SDS 3.1, this activity outputs `ArchivedReportUri`. If ArchiveReportAsync doesn't return a new one,
                // we might assume the existing GeneratedDocumentUri is now also the ArchivedReportUri, or it's an internal Data Service status.
                // For clarity in SagaData, let's set it.
                sagaData.ArchivedReportUri = sagaData.GeneratedDocumentUri; // Or a path returned by DataService if applicable.

                _logger.LogInformation("Report {ReportId} archived successfully. Archived URI (or reference): {ArchivedUri}", sagaData.ReportId, sagaData.ArchivedReportUri);
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error archiving report for ReportId: {ReportId}. Error: {ErrorMessage}", sagaData.ReportId, ex.Message);
                sagaData.FailureReason = $"Report Archiving failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore
            }
        }
    }
}