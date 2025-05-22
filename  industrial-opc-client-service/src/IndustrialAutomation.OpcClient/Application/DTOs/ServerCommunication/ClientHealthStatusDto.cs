using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record ClientHealthStatusDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
        public string OverallStatus { get; init; } = "Unknown"; // e.g., "Healthy", "Degraded", "Unhealthy"
        public Dictionary<string, string> ServerConnectionStatuses { get; init; } = new Dictionary<string, string>(); // Key: ServerId, Value: Status
        public Dictionary<string, string> SubscriptionStatuses { get; init; } = new Dictionary<string, string>(); // Key: SubscriptionId, Value: Status
        public long DataBufferSize { get; init; }
        public double CpuLoad { get; init; } // System CPU load if accessible
        public double MemoryUsage { get; init; } // Process memory usage
        public string? LastErrorMessage { get; init; }
        public Dictionary<string, string> ComponentDetails { get; init; } = new Dictionary<string, string>();
    }
}