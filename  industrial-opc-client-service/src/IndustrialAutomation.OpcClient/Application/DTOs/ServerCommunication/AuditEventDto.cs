using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record AuditEventDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
        public string EventType { get; init; } = string.Empty; // e.g., "AlarmAcknowledged", "ConfigApplied", "ServiceStart", "ServiceStop"
        public string Source { get; init; } = "OpcClientService"; // Component originating the event
        public string Description { get; init; } = string.Empty;
        public string? UserId { get; init; } // If applicable
        public Dictionary<string, string> Details { get; init; } = new Dictionary<string, string>(); // Key-value pairs for more context
    }
}