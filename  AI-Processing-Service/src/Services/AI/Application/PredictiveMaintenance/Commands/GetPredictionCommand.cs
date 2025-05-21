using MediatR;
using AIService.Application.PredictiveMaintenance.Models;
using System.Collections.Generic;

namespace AIService.Application.PredictiveMaintenance.Commands
{
    /// <summary>
    /// Represents a request to generate a maintenance prediction.
    /// Encapsulates necessary input data for the model.
    /// REQ-7-001: Core functionality for predictive maintenance.
    /// REQ-7-002: Input data requirements for models.
    /// </summary>
    public class GetPredictionCommand : IRequest<PredictionOutput>
    {
        /// <summary>
        /// The unique identifier of the AI model to be used for prediction.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the AI model. If null, the latest active version might be used.
        /// </summary>
        public string? ModelVersion { get; set; }

        /// <summary>
        /// A dictionary representing the input features for the prediction model.
        /// Keys are feature names, and values are the feature values.
        /// </summary>
        public Dictionary<string, object> Features { get; set; }

        public GetPredictionCommand(string modelId, Dictionary<string, object> features, string? modelVersion = null)
        {
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
            Features = features ?? throw new ArgumentNullException(nameof(features));
            ModelVersion = modelVersion;
        }
    }
}