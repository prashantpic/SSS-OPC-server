using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.AnomalyDetection
{
    /// <summary>
    /// Data transfer object for carrying input data for anomaly detection
    /// when using the REST API.
    /// REQ-7-008, REQ-7-009
    /// </summary>
    public class AnomalyDetectionRequestDto
    {
        /// <summary>
        /// Optional: The specific ID of the model to use for anomaly detection.
        /// If not provided, a default model may be used.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Input data for anomaly detection. This can be a single data point
        /// or a list of data points (e.g., a time series segment).
        /// Each data point is a dictionary of feature names and values.
        /// Example for single point: { "temperature": 80.0, "pressure": 100.1 }
        /// Example for multiple points: [ { "timestamp": "ts1", "value": 10 }, { "timestamp": "ts2", "value": 12 } ]
        /// For simplicity, this DTO uses a single Dictionary representing one data point or a set of features.
        /// For sequences, consider List<Dictionary<string, object>> or a more structured approach.
        /// </summary>
        [Required]
        public Dictionary<string, object> InputData { get; set; }

        // If you need to support a list/batch of data points directly in one request:
        // public List<Dictionary<string, object>> InputDataBatch { get; set; }
    }
}