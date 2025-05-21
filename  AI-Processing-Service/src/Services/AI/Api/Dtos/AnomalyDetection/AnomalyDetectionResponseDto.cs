using System;
using System.Collections.Generic;

namespace AIService.Api.Dtos.AnomalyDetection
{
    public class AnomalyDetectionResponseDto
    {
        /// <summary>
        /// A list of detected anomalies.
        /// REQ-7-008
        /// </summary>
        public List<AnomalyDto> Anomalies { get; set; }

        /// <summary>
        /// The ID of the model used for this anomaly detection process.
        /// </summary>
        public string ModelIdUsed { get; set; }

        /// <summary>
        /// A unique identifier for this specific anomaly detection execution.
        /// </summary>
        public string ExecutionId { get; set; }


        public AnomalyDetectionResponseDto()
        {
            Anomalies = new List<AnomalyDto>();
        }
    }

    public class AnomalyDto
    {
        /// <summary>
        /// Timestamp associated with the anomalous data point.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Severity score or level of the detected anomaly.
        /// </summary>
        public int Severity { get; set; }

        /// <summary>
        /// A description or type of the anomaly.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of feature names that contributed to this anomaly.
        /// </summary>
        public List<string> AffectedFeatures { get; set; }

        /// <summary>
        /// Confidence score of the anomaly detection.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Raw anomaly score from the model.
        /// </summary>
        public double RawScore { get; set; }

         public AnomalyDto()
        {
            AffectedFeatures = new List<string>();
        }
    }
}