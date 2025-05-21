using System;
using System.Collections.Generic;

namespace AIService.Api.Dtos.AnomalyDetection
{
    public class AnomalyDetectionResponseDto
    {
        public List<DetectedAnomalyDto> Anomalies { get; set; }
    }

    public class DetectedAnomalyDto
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public double SeverityScore { get; set; } // 0.0 to 1.0
        public List<string> ContributingFeatures { get; set; }
    }
}