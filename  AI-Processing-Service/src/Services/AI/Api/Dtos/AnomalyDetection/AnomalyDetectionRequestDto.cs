using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.AnomalyDetection
{
    public class AnomalyDetectionRequestDto
    {
        [Required]
        public string ModelId { get; set; }

        [Required]
        public List<TimeSeriesDataPointDto> DataPoints { get; set; }
    }

    public class TimeSeriesDataPointDto
    {
        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public Dictionary<string, double> Values { get; set; } // FeatureName -> FeatureValue
    }
}