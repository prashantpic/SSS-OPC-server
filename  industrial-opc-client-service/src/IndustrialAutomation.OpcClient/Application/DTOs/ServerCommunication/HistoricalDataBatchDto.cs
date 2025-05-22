using System;
using System.Collections.Generic;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record HistoricalDataBatchDto(
        string ClientId,
        string? QueryId, // Optional: to correlate with a specific request
        DateTime BatchTimestampUtc,
        List<OpcPointDto> HistoricalDataPoints // Can be from multiple tags
    );
}