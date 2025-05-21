using System.Collections.Generic;

namespace AIService.Application.PredictiveMaintenance.Models
{
    /// <summary>
    /// Represents the result of a prediction operation at the application layer,
    /// to be mapped to a DTO.
    /// REQ-7-001: Predictive Maintenance Analysis
    /// </summary>
    public class PredictionOutput
    {
        /// <summary>
        /// Indicates whether the prediction operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The predicted value or outcome. Could be a single value or a complex object.
        /// Represented as a dictionary for flexibility, aligning with ModelOutput.
        /// </summary>
        public Dictionary<string, object> PredictedValues { get; set; }

        /// <summary>
        /// Confidence score associated with the prediction, if applicable.
        /// </summary>
        public double? Confidence { get; set; }

        /// <summary>
        /// Any status message or additional information regarding the prediction.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Error message if the prediction operation failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        public PredictionOutput()
        {
            PredictedValues = new Dictionary<string, object>();
        }
    }
}