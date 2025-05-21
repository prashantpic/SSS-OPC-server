using System.Collections.Generic;

namespace OrchestrationService.Application.Events;

/// <summary>
/// Represents an event that triggers the blockchain synchronization saga.
/// Contains the critical data reference or payload requiring blockchain logging.
/// </summary>
public class BlockchainSyncRequestedEvent
{
    /// <summary>
    /// Reference or ID of the critical data to be synchronized.
    /// </summary>
    public string CriticalDataReference { get; set; } = string.Empty;

    /// <summary>
    /// Optional: Direct data payload if the data is small.
    /// </summary>
    public string? DataPayload { get; set; }

    /// <summary>
    /// Additional metadata related to the data or sync request.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Optional identifier of the event that triggered this request.
    /// </summary>
    public string? TriggeringEventId { get; set; }
}