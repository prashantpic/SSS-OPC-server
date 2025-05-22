using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaSubscriptionConfigDto(
        string SubscriptionId, // Client-defined ID for the subscription
        string ServerId,       // Identifies the OPC UA server connection
        double PublishingInterval, // Requested publishing interval in milliseconds
        int LifetimeCount,         // Requested lifetime count
        int MaxKeepAliveCount,     // Requested max keep-alive count
        int MaxNotificationsPerPublish, // Max notifications per publish
        bool PublishingEnabled,
        byte Priority,
        List<UaMonitoredItemDto> MonitoredItems
    );
}