using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.AnomalyDetection
{
    public class AnomalyDetectionRequestDto
    {
        /// <summary>
        /// Optional: The ID of the specific anomaly detection model to use.
        /// If not provided, a default or pre-configured model might be used.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// A list of data points to be analyzed for anomalies.
        /// Each data point is a dictionary of feature names and their values.
        /// REQ-7-009
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<Dictionary<string, object>> DataPoints { get; set; }

        public AnomalyDetectionRequestDto()
        {
            DataPoints = new List<Dictionary<string, object>>();
        }
    }
}