using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record HistoricalDataBatchDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService
        public string QueryId { get; init; } = Guid.NewGuid().ToString(); // Identifier for the HDA query
        public string OriginalServerId { get; init; } = string.Empty; // OPC HDA Server ID
        public DateTime BatchTimestampUtc { get; init; } = DateTime.UtcNow;
        // Data points might be for multiple tags from the same HDA server query
        public List<OpcPointDto> HistoricalDataPoints { get; init; } = new List<OpcPointDto>();
        public bool IsPartial { get; init; } = false; // True if this is part of a larger dataset for the query
        public bool IsComplete { get; init; } = true; // True if this is the final batch for the query (or only batch)
    }
}