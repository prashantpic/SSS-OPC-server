using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Ac
{
    public record AcAlarmEventDto(
        string EventId,       // Unique identifier for the event from the server
        string ServerId,      // Identifies the source OPC A&C server
        string SourceName,    // Name of the source of the event (e.g., tag name, area)
        string Message,       // Event message
        DateTime Timestamp,   // Time the event occurred or was reported by the server
        string Severity,      // Event severity (e.g., numeric string or textual like "High", "Low")
        string Category,      // Event category
        bool IsAcknowledged,
        bool IsConfirmed,
        string State,         // e.g., "Active", "Inactive", "Acknowledged", "Confirmed"
        string ConditionName, // Name of the condition associated with the event
        string? Comment,
        string? ActorId,       // User or system component that last acted on the event (e.g., for acknowledgement)
        Dictionary<string, object>? EventFields // For additional, non-standard event fields
    );
}