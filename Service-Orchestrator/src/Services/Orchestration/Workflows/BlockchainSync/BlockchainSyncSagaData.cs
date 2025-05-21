using System;
using System.Collections.Generic;

namespace OrchestrationService.Workflows.BlockchainSync
{
    /// <summary>
    /// Represents the stateful aggregate for an instance of the BlockchainSyncSaga.
    /// Holds persistent state such as original data identifiers, generated hashes,
    /// off-chain storage references, blockchain transaction IDs, and synchronization status.
    /// Implements REQ-8-007, REQ-DLP-025.
    /// </summary>
    public class BlockchainSyncSagaData
    {
        public string DataId { get; set; } = Guid.NewGuid().ToString();
        public BlockchainSyncSagaInput InputDataRef { get; set; } = new();
        public BlockchainSyncStatus CurrentStatus { get; set; } = BlockchainSyncStatus.Initiated;
        public string? DataHash { get; set; }
        public string? OffChainStoragePath { get; set; }
        public string? BlockchainTransactionId { get; set; }
        public string? FailureReason { get; set; }
        public List<string> CompensatedSteps { get; set; } = new();
    }

    public enum BlockchainSyncStatus
    {
        Initiated,
        PreparingData,
        DataPrepared,
        StoringOffChain,
        OffChainStorageCompleted,
        CommittingToBlockchain,
        CommitCompleted,
        Completed,
        Failed,
        Compensating,
        Compensated
    }
}