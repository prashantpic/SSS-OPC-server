using MediatR;
using System.Collections.Generic;
// Assuming a generic AnomalyDetectionResult model or specific list of anomalies
// For now, let's define a placeholder or use Dictionary similar to PredictionOutput

namespace AIService.Application.AnomalyDetection.Commands
{
    /// <summary>
    /// Represents a request to detect anomalies in a given dataset.
    /// REQ-7-008: Anomaly Detection
    /// REQ-7-009: Input data requirements for AD models
    /// </summary>
    public class DetectAnomaliesCommand : IRequest<DetectAnomaliesCommandResult> // Define DetectAnomaliesCommandResult later
    {
        /// <summary>
        /// The unique identifier of the AI model to be used for anomaly detection.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The specific version of the model to use.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Input data for the anomaly detection model.
        /// This could be a single data point or a batch, depending on the model.
        /// Keys are feature names, and values are the feature values.
        /// </summary>
        public Dictionary<string, object> InputData { get; set; } // Or List<Dictionary<string, object>> for batch

        public DetectAnomaliesCommand(string modelId, string modelVersion, Dictionary<string, object> inputData)
        {
            ModelId = modelId;
            ModelVersion = modelVersion;
            InputData = inputData ?? new Dictionary<string, object>();
        }
    }

    // Placeholder for the result type. This would typically be a more structured DTO/Model.
    // For example, it could be a list of detected anomalies with details.
    public class DetectAnomaliesCommandResult
    {
        public bool Success { get; set; }
        public List<DetectedAnomaly> Anomalies { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> RawOutput { get; set; } // For additional model output

        public DetectAnomaliesCommandResult()
        {
            Anomalies = new List<DetectedAnomaly>();
            RawOutput = new Dictionary<string, object>();
        }
    }

    public class DetectedAnomaly
    {
        public string AnomalyType { get; set; }
        public double SeverityScore { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> ContributingFactors { get; set; }
        public System.DateTime Timestamp { get; set; } // Timestamp of data point causing anomaly

        public DetectedAnomaly()
        {
            ContributingFactors = new Dictionary<string, object>();
        }
    }
}