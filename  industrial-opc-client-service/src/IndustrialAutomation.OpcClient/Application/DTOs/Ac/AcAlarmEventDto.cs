using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Ac
{
    public record AcAlarmEventDto
    {
        public string ServerId { get; init; } = string.Empty; // OPC Server that originated the event
        public string EventId { get; init; } = string.Empty; // Unique identifier for the event instance
        public string SourceName { get; init; } = string.Empty; // e.g., "MyDevice.TankLevel"
        public string? ConditionName { get; init; } // Name of the condition if applicable (e.g., "HighHighAlarm")
        public string Message { get; init; } = string.Empty; // Event message
        public DateTime Timestamp { get; init; } // Time the event occurred or was reported by the server
        public DateTime ActiveTime { get; init; } // Time the condition became active
        public string Severity { get; init; } = string.Empty; // e.g., "High", "Low", "500" (can be numeric string)
        public int SeverityValue { get; init; } // Numeric severity (OPC UA uses 1-1000)
        public string EventType { get; init; } = "Simple"; // "Simple", "Tracking", "Condition" (OPC A&E) or NodeId of EventType (OPC UA)
        public string Category { get; init; } = string.Empty; // Event category
        
        public bool IsAcknowledged { get; init; }
        public bool IsConfirmed { get; init; } // Mainly for OPC A&E
        public bool IsActive { get; init; }
        public bool Retain { get; init; } // UA: If true, event should be kept until explicitly deleted by history


        // State related fields (more common in OPC UA Conditions)
        public string? State { get; init; } // e.g., "Active", "Inactive", "Acked", "Unacked"
        public string? AckedState { get; init; } // e.g. "Acknowledged", "Unacknowledged" (UA: AckedState/Id)
        public string? ConfirmedState { get; init; } // e.g. "Confirmed", "Unconfirmed" (UA: ConfirmedState/Id)
        public string? ActiveState { get; init; } // e.g. "Active", "Inactive" (UA: ActiveState/Id)


        public string? AcknowledgerComment { get; init; }
        public string? AcknowledgerId { get; init; }
        public DateTime? AcknowledgeTime { get; init; }

        public string? Comment { get; init; } // General comment related to the event or condition state change
        public string? ClientUserId { get; init; } // User from client perspective if available

        public Dictionary<string, object> ExtendedProperties { get; init; } = new Dictionary<string, object>();
    }
}