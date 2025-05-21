using System.Collections.Generic;

namespace OrchestrationService.Workflows.BlockchainSync
{
    /// <summary>
    /// Defines the critical data payload (or reference to it) and associated metadata
    /// required to initiate the blockchain synchronization saga.
    /// Implements REQ-8-007.
    /// </summary>
    public class BlockchainSyncSagaInput
    {
        /// <summary>
        /// The critical data payload itself (e.g., serialized object, raw bytes).
        /// Use this if data is small enough to pass directly.
        /// </summary>
        public byte[]? CriticalDataPayload { get; set; }

        /// <summary>
        /// A reference (e.g., URI, ID) to retrieve the critical data if it's too large
        /// to pass directly in the input. The orchestrator or an early activity
        /// would be responsible for fetching this data.
        /// </summary>
        public string? CriticalDataReference { get; set; }

        public Dictionary<string, object> Metadata { get; set; } = new();
        public string RequestedBy { get; set; } = string.Empty; // User or system initiating
    }
}