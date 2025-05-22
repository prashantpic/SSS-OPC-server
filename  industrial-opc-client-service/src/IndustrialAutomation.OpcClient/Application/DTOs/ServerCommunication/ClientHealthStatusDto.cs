using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record ClientHealthStatusDto(
        string ClientId,
        DateTime TimestampUtc,
        string OverallStatus, // e.g., "Healthy", "Degraded", "Unhealthy"
        Dictionary<string, string> ServerConnectionStatuses, // Key: ServerId, Value: Status (e.g., "Connected", "Disconnected", "Error")
        Dictionary<string, string> SubscriptionStatuses, // Key: SubscriptionId, Value: Status
        long DataBufferSize, // Number of items currently in the buffer
        double CpuLoad,      // Client machine CPU load percentage
        double MemoryUsage   // Client machine memory usage MB or percentage
    );
}