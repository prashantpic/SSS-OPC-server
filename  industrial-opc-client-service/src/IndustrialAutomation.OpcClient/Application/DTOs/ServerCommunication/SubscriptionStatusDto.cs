using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record SubscriptionStatusDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService
        public string ServerId { get; init; } = string.Empty; // OPC Server ID
        public string SubscriptionId { get; init; } = string.Empty; // Client-defined subscription ID
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
        public string Status { get; init; } = "Unknown"; // e.g., "Active", "Connecting", "Error", "Lost"
        public string? LastErrorMessage { get; init; }
        public long DataChangeCount { get; init; } // Number of data changes received
        public long PublishRequestCount { get; init; }
        public long NotificationsCount { get; init; }
        public long KeepAliveCount { get; init; }
        public int MonitoredItemCount { get; init; }
        public double ActualPublishingInterval { get; init; }
    }
}