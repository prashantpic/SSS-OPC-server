using MediatR;
using AIService.Application.PredictiveMaintenance.Models;
using System.Collections.Generic;

namespace AIService.Application.PredictiveMaintenance.Commands
{
    /// <summary>
    /// Represents a request to generate a maintenance prediction, encapsulating necessary input data.
    /// Used in CQRS pattern.
    /// REQ-7-001: Predictive Maintenance Analysis
    /// REQ-7-002: Input data requirements for models
    /// </summary>
    public class GetPredictionCommand : IRequest<PredictionOutput>
    {
        /// <summary>
        /// The unique identifier of the AI model to be used for prediction.
        /// If null or empty, a default model might be used.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The specific version of the model to use.
        /// If null or empty, the latest or default version might be used.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Input data for the prediction model.
        /// Keys are feature names, and values are the feature values.
        /// This structure should align with the model's InputSchema.
        /// </summary>
        public Dictionary<string, object> InputData { get; set; }

        public GetPredictionCommand(string modelId, string modelVersion, Dictionary<string, object> inputData)
        {
            ModelId = modelId;
            ModelVersion = modelVersion;
            InputData = inputData ?? new Dictionary<string, object>();
        }
    }
}