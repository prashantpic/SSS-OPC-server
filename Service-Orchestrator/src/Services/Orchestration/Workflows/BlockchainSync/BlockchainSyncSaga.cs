using WorkflowCore.Interface;
using WorkflowCore.Models;
using OrchestrationService.Workflows.BlockchainSync.Activities;
using System;

namespace OrchestrationService.Workflows.BlockchainSync
{
    /// <summary>
    /// Defines the Saga for asynchronously synchronizing critical data to a permissioned blockchain.
    /// Orchestrates data preparation, off-chain storage of voluminous data, and on-chain commitment
    /// of data hashes, ensuring traceability and data integrity.
    /// Implements REQ-8-007, REQ-DLP-025.
    /// </summary>
    public class BlockchainSyncSaga : IWorkflow<BlockchainSyncSagaData>
    {
        public string Id => "BlockchainSyncSaga";
        public int Version => 1;

        public void Build(IWorkflowBuilder<BlockchainSyncSagaData> builder)
        {
            builder
                .StartWith<PrepareBlockchainDataActivity>()
                    .Input(step => step.InputDataRef, data => data.InputDataRef)
                    .Output(data => data.DataHash, step => step.Output.DataHash)
                .Then<StoreOffChainDataActivity>()
                    .Input(step => step.InputDataRef, data => data.InputDataRef)
                    .Output(data => data.OffChainStoragePath, step => step.Output.OffChainStoragePath)
                .Then<CommitToBlockchainActivity>()
                    .Input(step => step.DataHash, data => data.DataHash)
                    .Input(step => step.OffChainStoragePath, data => data.OffChainStoragePath)
                    .Input(step => step.Metadata, data => data.InputDataRef.Metadata)
                    .Output(data => data.BlockchainTransactionId, step => step.Output.BlockchainTransactionId)
                .EndWorkflow()
                .OnError(WorkflowErrorHandling.Retry, TimeSpan.FromMinutes(3)) // Global retry for transient issues
                .CompensateWith<CompensateBlockchainSyncActivity>(c =>
                {
                    c.Input(step => step.SagaDataAtFailure, data => data);
                });
        }
    }
}