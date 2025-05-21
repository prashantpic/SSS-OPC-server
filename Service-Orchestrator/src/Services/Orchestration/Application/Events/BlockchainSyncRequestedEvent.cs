using OrchestrationService.Workflows.BlockchainSync;

namespace OrchestrationService.Application.Events
{
    /// <summary>
    /// Represents an event that triggers the blockchain synchronization saga.
    /// Contains the critical data reference or payload requiring blockchain logging.
    /// Implements REQ-8-007.
    /// </summary>
    public class BlockchainSyncRequestedEvent
    {
        public BlockchainSyncSagaInput Input { get; set; } = new();
        public string? CorrelationId { get; set; }
        public string? CausationId { get; set; }
    }
}