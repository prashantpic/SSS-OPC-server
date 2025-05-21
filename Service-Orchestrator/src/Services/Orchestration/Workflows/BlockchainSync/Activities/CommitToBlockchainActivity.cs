using Microsoft.Extensions.Logging;
using OrchestrationService.Infrastructure.HttpClients.IntegrationService;
using OrchestrationService.Workflows.BlockchainSync.Models;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace OrchestrationService.Workflows.BlockchainSync.Activities
{
    /// <summary>
    /// Workflow activity to commit data hash and reference to the blockchain.
    /// Calls Integration Service to interact with smart contract.
    /// Corresponds to REQ-8-007.
    /// </summary>
    public class CommitToBlockchainActivity : StepBodyAsync
    {
        private readonly IIntegrationServiceClient _integrationServiceClient;
        private readonly ILogger<CommitToBlockchainActivity> _logger;

        public CommitToBlockchainActivity(IIntegrationServiceClient integrationServiceClient, ILogger<CommitToBlockchainActivity> logger)
        {
            _integrationServiceClient = integrationServiceClient;
            _logger = logger;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var sagaData = (BlockchainSyncSagaData)context.Workflow.Data;
            _logger.LogInformation("Committing data to blockchain for DataId: {DataId}, Hash: {DataHash}", sagaData.DataId, sagaData.DataHash);

            if (string.IsNullOrEmpty(sagaData.DataHash) || string.IsNullOrEmpty(sagaData.OffChainStoragePath))
            {
                _logger.LogError("DataHash or OffChainStoragePath is null/empty for DataId: {DataId}. Cannot commit to blockchain.", sagaData.DataId);
                sagaData.CurrentStatus = "Failed_CommitBlockchain_MissingParams";
                sagaData.FailureReason = "Data hash or off-chain storage path was not available for blockchain commit.";
                return ExecutionResult.Outcome("Error");
            }

            try
            {
                // The SDS mentions IIntegrationServiceClient.CommitToBlockchainAsync(hash, path, metadata)
                var blockchainMetadata = new BlockchainMetadata
                {
                    // Populate from sagaData.InputDataRef.Metadata or other relevant sagaData fields
                    SourceSystem = "ServiceOrchestrator", // Example
                    Timestamp = System.DateTime.UtcNow,
                    CustomData = sagaData.InputDataRef?.Metadata 
                };
                
                var transactionId = await _integrationServiceClient.CommitToBlockchainAsync(
                    sagaData.DataHash, 
                    sagaData.OffChainStoragePath, 
                    blockchainMetadata, 
                    context.CancellationToken);

                if (string.IsNullOrEmpty(transactionId))
                {
                    _logger.LogError("Blockchain transaction ID is null or empty for DataId: {DataId}.", sagaData.DataId);
                    sagaData.CurrentStatus = "Failed_CommitBlockchain_NoTxId";
                    sagaData.FailureReason = "Integration service did not return a valid blockchain transaction ID.";
                    // Depending on the integration, a null/empty TxID might mean it's queued or truly failed.
                    // If it can be queued, this might not be an immediate error. For now, assume failure.
                    return ExecutionResult.Outcome("Error");
                }

                sagaData.BlockchainTransactionId = transactionId;
                _logger.LogInformation("Data committed to blockchain successfully. Transaction ID: {BlockchainTransactionId} for DataId: {DataId}", sagaData.BlockchainTransactionId, sagaData.DataId);
                
                return ExecutionResult.Next();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error committing to blockchain for DataId: {DataId}. Error: {ErrorMessage}", sagaData.DataId, ex.Message);
                sagaData.FailureReason = $"Commit to Blockchain failed: {ex.Message}";
                throw; // Re-throw for WorkflowCore
            }
        }
    }
}