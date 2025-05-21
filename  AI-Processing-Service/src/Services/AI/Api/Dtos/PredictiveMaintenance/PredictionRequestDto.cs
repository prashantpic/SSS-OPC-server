using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.PredictiveMaintenance
{
    /// <summary>
    /// Data transfer object for carrying input data (e.g., sensor readings, operational parameters)
    /// required for a maintenance prediction when using the REST API.
    /// REQ-7-001, REQ-7-002
    /// </summary>
    public class PredictionRequestDto
    {
        /// <summary>
        /// Optional: The specific ID of the model to use for prediction.
        /// If not provided, a default model may be used.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Input data for the prediction model.
        /// This is a flexible dictionary where keys are feature names
        /// and values are the corresponding feature values.
        /// Example: { "temperature": 75.5, "pressure": 102.3, "vibration": 0.5 }
        /// </summary>
        [Required]
        public Dictionary<string, object> InputData { get; set; }
    }
}