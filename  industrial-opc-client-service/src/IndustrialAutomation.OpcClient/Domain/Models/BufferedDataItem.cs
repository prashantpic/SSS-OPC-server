using System;

namespace IndustrialAutomation.OpcClient.Domain.Models;

/// <summary>
/// Represents a data item (e.g., subscription update, alarm, AI output) 
/// queued in the local buffer for later transmission, typically due to network issues.
/// </summary>
public record BufferedDataItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string DataType { get; init; } // e.g., "RealtimeData", "AlarmEvent", "AiOutput", "CriticalWriteLog"
    public required object Payload { get; init; } // The actual DTO or data to be sent
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
}