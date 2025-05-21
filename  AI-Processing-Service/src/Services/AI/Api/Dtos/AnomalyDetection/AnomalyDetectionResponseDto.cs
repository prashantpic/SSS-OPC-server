using System;
using System.Collections.Generic;

namespace AIService.Api.Dtos.AnomalyDetection
{
    /// <summary>
    /// Represents a single detected anomaly.
    /// </summary>
    public class AnomalyDetailDto
    {
        /// <summary>
        /// Timestamp of the data point where the anomaly was detected.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Score indicating the severity or likelihood of the anomaly.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Optional description of the anomaly.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional identifier for the sensor or source of the anomalous data.
        /// </summary>
        public string SensorId { get; set; }

        /// <summary>
        /// The actual value or set of values that were deemed anomalous.
        /// Can be a simple value or a dictionary of features.
        /// </summary>
        public object Value { get; set; }
    }

    /// <summary>
    /// Data transfer object for returning detected anomalies and their details
    /// via the REST API.
    /// REQ-7-008
    /// </summary>
    public class AnomalyDetectionResponseDto
    {
        /// <summary>
        /// The ID of the model that was used for anomaly detection.
        /// </summary>
        public string ModelIdUsed { get; set; }

        /// <summary>
        /// Timestamp of when the anomaly detection process was completed.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// A list of detected anomalies.
        /// </summary>
        public List<AnomalyDetailDto> DetectedAnomalies { get; set; } = new List<AnomalyDetailDto>();
    }
}