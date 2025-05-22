namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaMonitoredItemDto
    {
        // Client's internal TagId, maps to a TagDefinitionDto.TagId
        // This helps correlate notifications back to a known tag configuration.
        public string ClientTagId { get; init; } = string.Empty;
        
        // The actual OPC UA NodeId string (e.g., "ns=2;s=MyDevice.Temperature")
        // This comes from TagDefinitionDto.OpcAddress for UA tags.
        public string NodeId { get; init; } = string.Empty; 

        public double SamplingInterval { get; init; } = -1; // milliseconds, -1 for default/inherited from subscription, 0 for fastest
        public string DataChangeTrigger { get; init; } = "StatusValue"; // "StatusValue", "StatusValueTimestamp"
        public uint QueueSize { get; init; } = 1; // 0 or 1 for latest value only, >1 for queueing
        public bool DiscardOldest { get; init; } = true; // If queue is full
        public Opc.Ua.MonitoringMode MonitoringMode { get; init; } = Opc.Ua.MonitoringMode.Reporting;
        public string? DisplayName { get; init; } // Optional, for easier identification
    }
}