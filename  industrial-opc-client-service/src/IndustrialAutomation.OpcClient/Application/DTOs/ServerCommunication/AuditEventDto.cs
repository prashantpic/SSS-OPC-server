using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record AuditEventDto(
        string ClientId,
        DateTime TimestampUtc,
        string EventType, // e.g., "AlarmAcknowledged", "ConfigurationChanged", "ServiceStartup"
        string Source,    // e.g., "OpcAcClient", "ConfigurationService", "System"
        string Description,
        Dictionary<string, string>? Details // Key-value pairs for additional event-specific data
    );
}