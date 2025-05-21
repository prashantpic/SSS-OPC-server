using System;
using System.Collections.Generic;

namespace OrchestrationService.Workflows.BlockchainSync;

/// <summary>
/// Represents the stateful aggregate for an instance of the BlockchainSyncSaga.
/// Holds persistent state such as original data identifiers, generated hashes,
/// off-chain storage references, blockchain transaction IDs, and synchronization status.
/// </summary>
public class BlockchainSyncSagaData
{
    /// <summary>
    /// Unique identifier for this specific synchronization process.
    /// </summary>
    public Guid SyncId { get; set; }

    /// <summary>
    /// Reference or ID of the critical data source being synchronized.
    /// </summary>
    public string OriginalDataReference { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the blockchain synchronization saga.
    /// Examples: "Initiated", "DataPrepared", "OffChainStorageComplete", "CommitInProgress", "BlockchainCommitted", "Failed", "Compensating".
    /// </summary>
    public string CurrentStatus { get; set; } = "Not Started";

    /// <summary>
    /// Cryptographic hash of the critical data.
    /// </summary>
    public string? DataHash { get; set; }

    /// <summary>
    /// Minimal data payload to be stored on-chain (often includes the hash and key identifiers).
    /// </summary>
    public string? OnChainPayload { get; set; } // Could be JSON or a specific format

    /// <summary>
    /// Reference or URI to the data stored off-chain (e.g., in a blob store or IPFS).
    /// </summary>
    public string? OffChainStoragePath { get; set; }

    /// <summary>
    /// ID of the committed blockchain transaction.
    /// </summary>
    public string? BlockchainTransactionId { get; set; }

    /// <summary>
    /// Stores error details if the saga fails.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Log of compensation actions taken (optional, for diagnostics).
    /// </summary>
    public List<string> CompensationLog { get; set; } = new List<string>();
}