using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.PredictiveMaintenance
{
    public class PredictionRequestDto
    {
        /// <summary>
        /// Optional: The ID of the specific model to use for prediction.
        /// If not provided, a default or pre-configured model might be used.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// A dictionary of input features (e.g., sensor readings, operational parameters)
        /// required by the predictive maintenance model.
        /// Keys are feature names, and values are the feature values.
        /// REQ-7-002
        /// </summary>
        [Required]
        public Dictionary<string, object> InputFeatures { get; set; }

        public PredictionRequestDto()
        {
            InputFeatures = new Dictionary<string, object>();
        }
    }
}