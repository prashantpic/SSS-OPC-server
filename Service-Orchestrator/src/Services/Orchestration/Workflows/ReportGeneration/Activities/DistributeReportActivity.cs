using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Infrastructure.HttpClients.ManagementService;
using OrchestrationService.Workflows.ReportGeneration.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.ReportGeneration.Activities
{
    /// <summary>
    /// Workflow activity for distributing generated reports.
    /// Corresponds to REQ-7-020, REQ-7-022.
    /// </summary>
    public class DistributeReportActivity : StepBodyAsync
    {
        private readonly IIntegrationServiceClient _integrationServiceClient;
        private readonly IDataServiceClient _dataServiceClient; // For saving to file locations
        private readonly IManagementServiceClient _managementServiceClient; // For RBAC/distribution lists
        private readonly ILogger<DistributeReportActivity> _logger;

        public DistributeReportActivity(
            IIntegrationServiceClient integrationServiceClient,
            IDataServiceClient dataServiceClient,
            IManagementServiceClient managementServiceClient,
            ILogger<DistributeReportActivity> logger)
        {
            _integrationServiceClient = integrationServiceClient;
            _dataServiceClient = dataServiceClient;
            _managementServiceClient = managementServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (ReportGenerationSagaData)context.Workflow.Data;
            _logger.LogInformation("Distributing report for ReportId: {ReportId}", sagaData.ReportId);

            if (string.IsNullOrEmpty(sagaData.GeneratedDocumentUri) || sagaData.RequestParameters == null || string.IsNullOrEmpty(sagaData.RequestParameters.DistributionTarget))
            {
                _logger.LogError("Missing GeneratedDocumentUri or DistributionTarget for ReportId: {ReportId}. Cannot distribute.", sagaData.ReportId);
                sagaData.CurrentStatus = "Failed_Distribution_MissingParams";
                sagaData.FailureReason = "Missing document URI or distribution target for report distribution.";
                return ExecutionResult.Outcome("Error");
            }

            try
            {
                // Example: DistributionTarget might be an email, a user group ID, or a file path prefix.
                // This logic would determine how to interpret DistributionTarget.
                // For simplicity, let's assume it's an email or a hint for service.
                
                // The SDS mentions IIntegrationServiceClient.DistributeReportAsync(uri, target) or IDataServiceClient.StoreReportAsync(uri, path)
                // It also mentions IManagementServiceClient for RBAC/distribution lists.

                // Let's define a placeholder DistributionTargetDetails for IIntegrationServiceClient
                var distributionDetails = new DistributionTargetDetails
                {
                    TargetIdentifier = sagaData.RequestParameters.DistributionTarget,
                    // Potentially fetch more details from ManagementService if TargetIdentifier is a group ID
                    // e.g., if sagaData.RequestParameters.DistributionTarget is a group ID:
                    // var groupDetails = await _managementServiceClient.GetDistributionDetailsAsync(sagaData.RequestParameters.DistributionTarget);
                    // distributionDetails.Recipients = groupDetails.Emails; // or similar
                };


                // Simplified decision logic, could be more complex based on target type
                if (sagaData.RequestParameters.DistributionTarget.Contains("@")) // Heuristic for email
                {
                    await _integrationServiceClient.DistributeReportAsync(sagaData.GeneratedDocumentUri, distributionDetails, context.CancellationToken);
                    _logger.LogInformation("Report {ReportId} distribution initiated via IntegrationService to {Target}", sagaData.ReportId, sagaData.RequestParameters.DistributionTarget);
                }
                else // Assume file path or other target handled by DataService or a more specific IntegrationService call
                {
                    // This part is more ambiguous in SDS. Let's assume DataService for storing to a path if not email-like.
                    // await _dataServiceClient.StoreReportAsync(sagaData.GeneratedDocumentUri, sagaData.RequestParameters.DistributionTarget, context.CancellationToken);
                    // For now, let's log a warning if it's not an email, as the above StoreReportAsync is not in IDataServiceClient as per SDS.
                    // The SDS for IDataServiceClient has `ArchiveReportAsync` and `StoreOffChainDataAsync`, but not a generic `StoreReportAsync`.
                    // The SDS for IIntegrationServiceClient has `DistributeReportAsync` which is more general.
                    // Let's assume `DistributeReportAsync` in `IIntegrationServiceClient` handles various targets.
                    await _integrationServiceClient.DistributeReportAsync(sagaData.GeneratedDocumentUri, distributionDetails, context.CancellationToken);
                    _logger.LogInformation("Report {ReportId} distribution (non-email) initiated via IntegrationService to {Target}", sagaData.ReportId, sagaData.RequestParameters.DistributionTarget);
                }
                
                // Add to distribution list in saga data if needed
                if (sagaData.DistributionList == null) sagaData.DistributionList = new System.Collections.Generic.List<string>();
                sagaData.DistributionList.Add(sagaData.RequestParameters.DistributionTarget);

                _logger.LogInformation("Report distribution completed for ReportId: {ReportId} to {Target}", sagaData.ReportId, sagaData.RequestParameters.DistributionTarget);
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error distributing report for ReportId: {ReportId}. Error: {ErrorMessage}", sagaData.ReportId, ex.Message);
                sagaData.FailureReason = $"Report Distribution failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore
            }
        }
    }
}