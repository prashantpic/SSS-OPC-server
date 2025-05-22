using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Hda
{
    public record HdaReadRequestDto
    {
        public string ServerId { get; init; } = string.Empty;
        public List<string> TagIds { get; init; } = new List<string>(); // Client's internal TagIds
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public string DataRetrievalMode { get; init; } = "Raw"; // "Raw", "Processed", "Interpolated", "AtTime"
        
        // For Processed or Interpolated
        public string? AggregationType { get; init; } // e.g., "Average", "Minimum", "Maximum", "Count", "Interpolative"
                                                     // Specific values depend on HDA server capabilities (e.g. Opc.Hda.AggregateType)
        public double ResampleIntervalMs { get; init; } = 0; // For processed data, resample interval in milliseconds

        // For Raw
        public uint MaxValuesPerTagReturned { get; init; } = 0; // 0 for no limit by client (server limit may apply)
        public bool IncludeBounds { get; init; } = false; // Include values at StartTime and EndTime if available

        // For AtTime
        public List<DateTime>? Timestamps { get; init; } // List of specific timestamps to read data for

        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    }
}