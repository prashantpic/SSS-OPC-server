using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Hda
{
    public record HdaReadRequestDto(
        string ServerId,
        List<string> TagIds, // Internal TagIds, to be mapped to HDA ItemIDs
        DateTime StartTime,
        DateTime EndTime,
        string DataRetrievalMode, // "Raw", "Processed", "Interpolated", "AtTime" etc.
        string? AggregationType,   // e.g., "Average", "Min", "Max", "Count" - relevant for "Processed" mode
        double ResampleIntervalMs = 0 // Relevant for "Processed" or "Interpolated" mode
    );
}