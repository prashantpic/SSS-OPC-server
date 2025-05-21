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
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the AI model.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Input data for the model, as a collection of key-value pairs.
        /// The structure should conform to the model's expected input schema.
        /// </summary>
        public Dictionary<string, object> InputData { get; set; }

        public GetPredictionCommand(string modelId, string modelVersion, Dictionary<string, object> inputData)
        {
            ModelId = modelId;
            ModelVersion = modelVersion;
            InputData = inputData;
        }
    }
}