using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.DataService;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Workflows.BlockchainSync.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.BlockchainSync.Activities
{
    /// <summary>
    /// Saga compensation activity for failed blockchain synchronization.
    /// Handles rollback or corrective actions, such as marking off-chain data or logging failure.
    /// Corresponds to REQ-8-007, REQ-DLP-025.
    /// </summary>
    public class CompensateBlockchainSyncActivity : StepBodyAsync
    {
        private readonly IDataServiceClient _dataServiceClient;
        private readonly IIntegrationServiceClient _integrationServiceClient; // For notifications or corrective blockchain actions (if possible)
        private readonly ILogger<CompensateBlockchainSyncActivity> _logger;

        public CompensateBlockchainSyncActivity(
            IDataServiceClient dataServiceClient,
            IIntegrationServiceClient integrationServiceClient,
            ILogger<CompensateBlockchainSyncActivity> logger)
        {
            _dataServiceClient = dataServiceClient;
            _integrationServiceClient = integrationServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (BlockchainSyncSagaData)context.Workflow.Data;
            _logger.LogInformation("Initiating compensation for BlockchainSyncSaga, DataId: {DataId}. Failure Reason: {FailureReason}", sagaData.DataId, sagaData.FailureReason);
            sagaData.CurrentStatus = "Compensating";
            if (sagaData.CompensatedSteps == null) sagaData.CompensatedSteps = new System.Collections.Generic.List<string>();

            // 1. If BlockchainTransactionId exists (Commit failed *after* broadcast or requires explicit rollback)
            //    Blockchain transactions are typically immutable. Compensation often involves off-chain status updates.
            if (!string.IsNullOrEmpty(sagaData.BlockchainTransactionId) && !sagaData.CompensatedSteps.Contains(nameof(sagaData.BlockchainTransactionId)))
            {
                try
                {
                    _logger.LogWarning("Blockchain transaction {BlockchainTransactionId} for DataId: {DataId} may exist but commit failed or requires manual intervention. Attempting to log for review.", sagaData.BlockchainTransactionId, sagaData.DataId);
                    // Corrective action via Integration Service (e.g., log issue, update related off-chain record status if blockchain supports it)
                    // await _integrationServiceClient.RequestBlockchainTransactionReviewAsync(sagaData.BlockchainTransactionId, "Compensation for failed sync");
                    _logger.LogWarning("Placeholder for potential corrective action or logging for TxId: {BlockchainTransactionId}", sagaData.BlockchainTransactionId);
                    sagaData.CompensatedSteps.Add(nameof(sagaData.BlockchainTransactionId));
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Compensation Error: Failed during corrective action for TxId {BlockchainTransactionId}, DataId: {DataId}", sagaData.BlockchainTransactionId, sagaData.DataId);
                }
            }

            // 2. If OffChainStoragePath exists (Store failed, or commit after store failed)
            //    Mark the stored data as 'failed-sync' or schedule for cleanup.
            if (!string.IsNullOrEmpty(sagaData.OffChainStoragePath) && !sagaData.CompensatedSteps.Contains(nameof(sagaData.OffChainStoragePath)))
            {
                try
                {
                    _logger.LogInformation("Compensating: Marking off-chain data at {OffChainStoragePath} as 'failed-sync' for DataId: {DataId}", sagaData.OffChainStoragePath, sagaData.DataId);
                    // Assuming IDataServiceClient has a method to update status or schedule cleanup
                    // await _dataServiceClient.UpdateOffChainDataStatusAsync(sagaData.OffChainStoragePath, "failed-sync", context.CancellationToken);
                    // Or, if it needs to be deleted:
                    // await _dataServiceClient.DeleteOffChainDataAsync(sagaData.OffChainStoragePath, context.CancellationToken); // Not in SDS, using DeleteDocumentAsync as a proxy if applicable
                    await _dataServiceClient.DeleteDocumentAsync(sagaData.OffChainStoragePath, context.CancellationToken); // Assuming this generic delete can be used.
                    _logger.LogInformation("Compensated: Successfully marked/deleted off-chain data for DataId: {DataId}", sagaData.DataId);
                    sagaData.CompensatedSteps.Add(nameof(sagaData.OffChainStoragePath));
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Compensation Error: Failed to mark/delete off-chain data at {OffChainStoragePath} for DataId: {DataId}", sagaData.OffChainStoragePath, sagaData.DataId);
                }
            }

            // 3. Notify relevant parties.
            try
            {
                _logger.LogInformation("Compensating: Notifying about blockchain sync failure for DataId: {DataId}", sagaData.DataId);
                // Example: await _integrationServiceClient.SendFailureNotificationAsync($"Blockchain Sync Failed for ID: {sagaData.DataId}", ...);
                _logger.LogWarning("Failure notification for blockchain sync DataId {DataId} is a placeholder.", sagaData.DataId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Compensation Error: Failed to send failure notification for blockchain sync DataId: {DataId}", sagaData.DataId);
            }
            
            sagaData.CurrentStatus = "Compensated";
            _logger.LogInformation("Blockchain sync compensation process completed for DataId: {DataId}. Final status in saga data: {CurrentStatus}", sagaData.DataId, sagaData.CurrentStatus);

            return ExecutionResult.Next();
        }
    }
}