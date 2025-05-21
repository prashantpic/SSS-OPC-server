using System;
using System.Collections.Generic;

namespace AIService.Api.Dtos.PredictiveMaintenance
{
    public class PredictionResponseDto
    {
        /// <summary>
        /// A unique identifier for this specific prediction.
        /// </summary>
        public string PredictionId { get; set; }

        /// <summary>
        /// A dictionary containing the prediction results.
        /// Keys might be "RemainingUsefulLife", "FailureProbability", etc.,
        /// and values are the corresponding predicted values.
        /// REQ-7-001
        /// </summary>
        public Dictionary<string, object> Results { get; set; }

        /// <summary>
        /// The version of the AI model that was used to generate this prediction.
        /// </summary>
        public string ModelVersionUsed { get; set; }

        /// <summary>
        /// Timestamp of when the prediction was generated.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public PredictionResponseDto()
        {
            Results = new Dictionary<string, object>();
        }
    }
}