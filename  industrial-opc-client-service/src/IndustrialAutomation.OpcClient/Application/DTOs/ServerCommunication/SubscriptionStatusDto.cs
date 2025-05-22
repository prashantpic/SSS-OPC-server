using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record SubscriptionStatusDto(
        string ClientId,
        string SubscriptionId,
        string ServerId, // The server this subscription belongs to
        DateTime TimestampUtc,
        string Status, // e.g., "Active", "Connecting", "Disconnected", "Error", "NoConfig"
        string? LastErrorMessage,
        int DataChangeCount, // Count of data changes received in a recent interval or since last report
        int MonitoredItemCount,
        int QueueOverflowCount
    );
}