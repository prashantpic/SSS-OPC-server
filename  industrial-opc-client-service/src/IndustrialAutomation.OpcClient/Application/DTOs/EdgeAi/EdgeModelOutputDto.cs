using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi
{
    public record EdgeModelOutputDto
    {
        public string ModelName { get; init; } = string.Empty;
        public string ModelVersion { get; init; } = string.Empty;
        public DateTime InferenceTimestampUtc { get; init; } = DateTime.UtcNow;
        public Dictionary<string, object> Results { get; init; } = new Dictionary<string, object>();
        // Example: Results["prediction"] = 1, Results["anomaly_score"] = 0.95
        public string Status { get; init; } = "Success"; // "Success", "Error"
        public string? ErrorMessage { get; init; }
        public Dictionary<string, object>? Metadata { get; init; } // e.g., confidence scores, explanations
    }
}