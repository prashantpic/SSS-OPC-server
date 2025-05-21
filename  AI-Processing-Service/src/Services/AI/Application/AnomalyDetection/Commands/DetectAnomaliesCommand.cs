using MediatR;
using System.Collections.Generic;
using AIService.Application.AnomalyDetection.Models; // Assuming this namespace for result type

namespace AIService.Application.AnomalyDetection.Commands
{
    /// <summary>
    /// Represents a request to detect anomalies in a given dataset.
    /// REQ-7-008: Anomaly Detection
    /// REQ-7-009: Input data requirements for AD models
    /// </summary>
    public class DetectAnomaliesCommand : IRequest<AnomalyDetectionResult>
    {
        /// <summary>
        /// The unique identifier of the AI model to be used for anomaly detection.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the AI model.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Input data for the model, as a collection of key-value pairs or a list of data points.
        /// The structure should conform to the model's expected input schema.
        /// For simplicity, using Dictionary, but could be List<Dictionary<string, object>> for multiple data points.
        /// </summary>
        public Dictionary<string, object> InputData { get; set; } // Or List<Dictionary<string, object>>

        public DetectAnomaliesCommand(string modelId, string modelVersion, Dictionary<string, object> inputData)
        {
            ModelId = modelId;
            ModelVersion = modelVersion;
            InputData = inputData;
        }
    }
}