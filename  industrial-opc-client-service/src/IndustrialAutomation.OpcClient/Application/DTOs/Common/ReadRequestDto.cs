using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record ReadRequestDto
    {
        public string ServerId { get; init; } = string.Empty;
        public List<string> TagIds { get; init; } = new List<string>(); // Client's internal TagIds
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    }
}