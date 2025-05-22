using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record OpcPointDto
    {
        public string TagId { get; init; } = string.Empty;
        public object? Value { get; init; }
        public DateTime Timestamp { get; init; }
        public string QualityStatus { get; init; } = string.Empty;
    }
}