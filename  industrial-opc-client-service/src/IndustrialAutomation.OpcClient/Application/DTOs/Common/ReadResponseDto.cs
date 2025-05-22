using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record ReadResponseDto
    {
        public string ServerId { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = string.Empty;
        public List<OpcPointDto> Values { get; init; } = new List<OpcPointDto>();
        public bool Success { get; init; }
        public string StatusCode { get; init; } = string.Empty; // Overall status for the batch read
        public string? ErrorMessage { get; init; }
    }
}