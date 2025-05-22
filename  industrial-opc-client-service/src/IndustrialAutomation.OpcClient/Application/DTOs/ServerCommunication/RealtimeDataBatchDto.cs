using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record RealtimeDataBatchDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService
        public DateTime BatchTimestampUtc { get; init; } = DateTime.UtcNow;
        public List<OpcPointDto> DataPoints { get; init; } = new List<OpcPointDto>();
    }
}