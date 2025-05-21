using WorkflowCore.Interface;
using OrchestrationService.Workflows.BlockchainSync.Activities;
using System;

namespace OrchestrationService.Workflows.BlockchainSync;

/// <summary>
/// Defines the Saga for asynchronously synchronizing critical data to a permissioned blockchain.
/// Orchestrates data preparation, off-chain storage of voluminous data, and on-chain commitment
/// of data hashes, ensuring traceability and data integrity.
/// </summary>
public class BlockchainSyncSaga : IWorkflow<BlockchainSyncSagaData>
{
    public string Id => "BlockchainSyncSaga"; // Unique ID for this workflow definition
    public int Version => 1; // Version of the workflow definition

    /// <summary>
    /// Builds the workflow definition.
    /// </summary>
    /// <param name="builder">The workflow builder instance.</param>
    public void Build(IWorkflowBuilder<BlockchainSyncSagaData> builder)
    {
        builder
            .StartWith(context =>
            {
                context.Workflow.Data.SyncId = Guid.NewGuid(); // Initialize a unique ID for this sync process
                context.Workflow.Data.CurrentStatus = "Initiated";
                Console.WriteLine($"Blockchain Sync Saga {context.Workflow.Id} (SyncId: {context.Workflow.Data.SyncId}) initiated.");
                return ExecutionResult.Next();
            })
            .Then<PrepareBlockchainDataActivity>()
                .Input(step => step.Input = step.Workflow.Data.OriginalDataReference) // Pass the critical data reference
                .Output(step => { // Update SagaData with prepared hash and on-chain payload
                    step.Workflow.Data.DataHash = step.Output.DataHash;
                    step.Workflow.Data.OnChainPayload = step.Output.OnChainPayload;
                    step.Workflow.Data.CurrentStatus = "DataPrepared";
                })
                .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(10))
                .CompensateWith<CompensateBlockchainSyncActivity>() // Compensation for data preparation is usually minimal
            .Then<StoreOffChainDataActivity>()
                .Input(step => step.Input = step.Workflow.Data) // Pass the full SagaData for context
                .Output(step => {
                    step.Workflow.Data.OffChainStoragePath = step.Output;
                    step.Workflow.Data.CurrentStatus = "OffChainDataStored";
                })
                .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(15))
                .CompensateWith<CompensateBlockchainSyncActivity>() // If commit fails, compensate off-chain storage
            .Then<CommitToBlockchainActivity>()
                 .Input(step => step.Input = step.Workflow.Data) // Pass the full SagaData
                 .Output(step => {
                     step.Workflow.Data.BlockchainTransactionId = step.Output;
                     step.Workflow.Data.CurrentStatus = "BlockchainCommitted";
                 })
                 .OnError(WorkflowCore.Models.WorkflowErrorHandling.Retry, TimeSpan.FromSeconds(30)) // Longer retry for blockchain interaction
                 // Compensation for CommitToBlockchain is tricky; actual blockchain tx cannot be undone.
                 // Compensation involves logging failure, alerting, or attempting alternative actions.
                 // The CompensateBlockchainSyncActivity should handle this based on current state.
                 .CompensateWith<CompensateBlockchainSyncActivity>()
            .Then(context =>
            {
                context.Workflow.Data.CurrentStatus = "Completed";
                Console.WriteLine($"Blockchain Sync Saga {context.Workflow.Id} (SyncId: {context.Workflow.Data.SyncId}) completed. TxId: {context.Workflow.Data.BlockchainTransactionId}");
                return ExecutionResult.Next();
            })
            .OnError(WorkflowCore.Models.WorkflowErrorHandling.Compensate); // Global error handling: trigger compensation
    }
}