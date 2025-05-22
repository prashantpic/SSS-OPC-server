using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record WriteRequestDto
    {
        public string ServerId { get; init; } = string.Empty;
        public string TagId { get; init; } = string.Empty; // Client's internal TagId
        public object Value { get; init; } = new(); // Value to write
        public DateTime? Timestamp { get; init; } // Optional: for OPC UA source timestamp
        public string? InitiatingUser { get; init; } // For audit purposes
        public string? Context { get; init; } // Additional context for the write operation
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    }
}