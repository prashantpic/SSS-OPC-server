namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaMonitoredItemDto(
        string ItemId,         // Client-defined unique ID for this monitored item (can be same as TagId)
        string NodeId,         // The OPC UA NodeId string to monitor
        double SamplingInterval, // Requested sampling interval in milliseconds (0 for server default/fastest)
        string DataChangeTrigger, // e.g., "StatusValue", "StatusValueTimestamp" (maps to Opc.Ua.DataChangeTrigger)
        int QueueSize = 1,        // Client-side queue size for notifications (0 or 1 for latest only)
        bool DiscardOldest = true // If queue overflows, discard oldest or newest
    );
}