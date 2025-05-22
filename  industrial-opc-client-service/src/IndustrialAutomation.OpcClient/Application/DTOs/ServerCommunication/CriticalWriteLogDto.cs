using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record CriticalWriteLogDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService or Logger
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
        public string ServerId { get; init; } = string.Empty; // OPC Server ID
        public string TagId { get; init; } = string.Empty; // Client's internal TagId
        public string OpcAddress { get; init; } = string.Empty; // Actual OPC address (NodeId/ItemId)
        public object? OldValue { get; init; }
        public object NewValue { get; init; } = new();
        public string InitiatingUser { get; init; } = "System";
        public string? Context { get; init; }
        public bool Success { get; init; }
        public string StatusCode { get; init; } = string.Empty; // OPC status code of the write operation
        public string? ErrorMessage { get; init; }
        public string CorrelationId { get; init; } = string.Empty;
    }
}