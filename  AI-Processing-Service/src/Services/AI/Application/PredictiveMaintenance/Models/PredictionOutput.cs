using System.Collections.Generic;

namespace AIService.Application.PredictiveMaintenance.Models
{
    /// <summary>
    /// Represents the result of a prediction operation at the application layer,
    /// to be mapped to a DTO for API response.
    /// REQ-7-001: Predictive Maintenance Analysis
    /// </summary>
    public class PredictionOutput
    {
        /// <summary>
        /// Indicates whether the prediction was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The raw output from the model. Keys are output names, values are the results.
        /// This allows for flexibility with different model output structures.
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; }

        /// <summary>
        /// Error message if the prediction was not successful.
        /// </summary>
        public string ErrorMessage { get; set; }

        // Example of specific properties if models have common outputs:
        // public double? RemainingUsefulLife { get; set; }
        // public double? ProbabilityOfFailure { get; set; }
        // public string SuggestedActions { get; set; }

        public PredictionOutput()
        {
            Outputs = new Dictionary<string, object>();
        }
    }
}