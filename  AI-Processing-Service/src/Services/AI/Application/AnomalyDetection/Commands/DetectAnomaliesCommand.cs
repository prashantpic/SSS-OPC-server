using MediatR;
using System.Collections.Generic;
using AIService.Application.AnomalyDetection.Models; // Assuming AnomalyDetails will be here

namespace AIService.Application.AnomalyDetection.Commands
{
    /// <summary>
    /// Represents a request to detect anomalies in a given dataset using a specified AI model.
    /// REQ-7-008: Core functionality for anomaly detection.
    /// REQ-7-009: Input data requirements for anomaly detection models.
    /// </summary>
    public class DetectAnomaliesCommand : IRequest<IEnumerable<AnomalyDetails>>
    {
        /// <summary>
        /// The unique identifier of the AI model to be used for anomaly detection.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the AI model. If null, the latest active version might be used.
        /// </summary>
        public string? ModelVersion { get; set; }

        /// <summary>
        /// A list of data points to be analyzed for anomalies. 
        /// Each data point is a dictionary of feature names and their values.
        /// For models processing single instance, this list might contain one item.
        /// For models processing batches or sequences, this will contain multiple items.
        /// </summary>
        public List<Dictionary<string, object>> DataPoints { get; set; }

        public DetectAnomaliesCommand(string modelId, List<Dictionary<string, object>> dataPoints, string? modelVersion = null)
        {
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
            DataPoints = dataPoints ?? throw new ArgumentNullException(nameof(dataPoints));
            ModelVersion = modelVersion;
        }
    }
}