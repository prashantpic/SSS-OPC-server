using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Hda
{
    public record HdaReadResponseDto
    {
        public string ServerId { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = string.Empty;
        // Using a dictionary where key is TagId and value is a list of its historical points
        public Dictionary<string, List<OpcPointDto>> HistoricalData { get; init; } = new Dictionary<string, List<OpcPointDto>>();
        public bool Success { get; init; }
        public string StatusCode { get; init; } = string.Empty; // Overall status for the HDA read operation
        public string? ErrorMessage { get; init; }
    }
}