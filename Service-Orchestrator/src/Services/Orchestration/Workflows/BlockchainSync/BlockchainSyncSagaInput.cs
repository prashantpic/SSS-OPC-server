using System.Collections.Generic;

namespace OrchestrationService.Workflows.BlockchainSync;

/// <summary>
/// Defines the critical data payload (or reference to it) and associated metadata
/// required to initiate the blockchain synchronization saga.
/// </summary>
public class BlockchainSyncSagaInput
{
    /// <summary>
    /// Reference or ID of the critical data to be synchronized.
    /// This could be an identifier to fetch the data or the data itself if small.
    /// </summary>
    public string CriticalDataReference { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Direct data payload if the data is not voluminous and can be passed directly.
    /// If provided, CriticalDataReference might be a descriptive name or ID.
    /// </summary>
    public string? DataPayload { get; set; } // E.g., JSON string of the data

    /// <summary>
    /// Additional metadata related to the data or the synchronization request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Optional: Identifier of the event or external trigger that initiated this sync request.
    /// </summary>
    public string? TriggeringEventId { get; set; }
}