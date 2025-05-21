using System.Collections.Generic;

namespace AIService.Application.PredictiveMaintenance.Models
{
    /// <summary>
    /// Represents the result of a prediction operation at the application layer.
    /// This model is typically mapped to a DTO for API responses.
    /// REQ-7-001: Output for predictive maintenance analysis.
    /// </summary>
    public class PredictionOutput
    {
        /// <summary>
        /// A dictionary containing the prediction results. 
        /// Keys are output names (e.g., "RemainingUsefulLife", "FailureProbability"), 
        /// and values are the predicted values.
        /// </summary>
        public Dictionary<string, object> Predictions { get; set; }

        /// <summary>
        /// Optional: Overall confidence score of the prediction, if applicable.
        /// </summary>
        public double? ConfidenceScore { get; set; }

        /// <summary>
        /// Optional: Identifier of the model version that produced this prediction.
        /// </summary>
        public string? ModelVersionUsed { get; set; }

        /// <summary>
        /// Optional: Any additional metadata or explanation related to the prediction.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        public PredictionOutput()
        {
            Predictions = new Dictionary<string, object>();
            Metadata = new Dictionary<string, string>();
        }
    }
}