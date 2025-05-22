using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi
{
    public record EdgeModelInputDto
    {
        public string ModelName { get; set; } = string.Empty; // Ensure this is set before passing to executor
        public string ModelVersion { get; set; } = string.Empty; // Ensure this is set
        public Dictionary<string, object> Features { get; init; } = new Dictionary<string, object>();
        // Example: Features["temperature"] = 25.5, Features["pressure"] = 1012.3
        public DateTime TimestampUtc { get; init; } = DateTime.UtcNow; // Timestamp of the input data
    }
}