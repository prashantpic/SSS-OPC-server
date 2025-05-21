using System;
using System.Collections.Generic;

namespace AIService.Api.Dtos.PredictiveMaintenance
{
    /// <summary>
    /// Data transfer object for returning the outcome of a maintenance prediction
    /// (e.g., remaining useful life, failure probability) via the REST API.
    /// REQ-7-001
    /// </summary>
    public class PredictionResponseDto
    {
        /// <summary>
        /// A unique identifier for this specific prediction instance.
        /// </summary>
        public string PredictionId { get; set; }

        /// <summary>
        /// The ID of the model that was used to generate this prediction.
        /// </summary>
        public string ModelIdUsed { get; set; }

        /// <summary>
        /// Timestamp of when the prediction was generated.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The prediction results.
        /// This is a flexible dictionary where keys are output names
        /// (e.g., "RemainingUsefulLife", "FailureProbability")
        /// and values are the predicted values.
        /// </summary>
        public Dictionary<string, object> Results { get; set; }
    }
}