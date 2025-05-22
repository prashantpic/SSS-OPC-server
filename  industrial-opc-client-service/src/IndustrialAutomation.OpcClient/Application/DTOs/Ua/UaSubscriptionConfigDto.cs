using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaSubscriptionConfigDto
    {
        public string SubscriptionId { get; init; } = Guid.NewGuid().ToString(); // Client-defined ID for the subscription
        public string ServerId { get; init; } = string.Empty; // OPC Server this subscription belongs to
        public double PublishingInterval { get; init; } = 1000; // In milliseconds
        public uint LifetimeCount { get; init; } = 1200; // PublishingIntervals * 10 * 3 (OPC UA Spec recommendation)
        public uint MaxKeepAliveCount { get; init; } = 10; // (OPC UA Spec recommendation)
        public uint MaxNotificationsPerPublish { get; init; } = 0; // 0 for no limit
        public bool PublishingEnabled { get; init; } = true;
        public byte Priority { get; init; } = 0; // 0-255
        public List<UaMonitoredItemDto> MonitoredItems { get; init; } = new List<UaMonitoredItemDto>();
    }
}