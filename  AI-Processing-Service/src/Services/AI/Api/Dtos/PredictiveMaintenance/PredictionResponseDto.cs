using System.Collections.Generic;

namespace AIService.Api.Dtos.PredictiveMaintenance
{
    public class PredictionResponseDto
    {
        public string PredictionId { get; set; }
        public Dictionary<string, object> OutputData { get; set; }
        public string Status { get; set; }
        // Example specific fields if known:
        // public double? RemainingUsefulLife { get; set; }
        // public double? FailureProbability { get; set; }
    }
}