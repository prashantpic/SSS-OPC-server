using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.ModelManagement
{
    public class ModelFeedbackRequestDto
    {
        /// <summary>
        /// The ID of the model for which feedback is being provided.
        /// </summary>
        [Required]
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the model.
        /// </summary>
        [Required]
        public string ModelVersion { get; set; }

        /// <summary>
        /// Optional: The unique ID of the prediction this feedback pertains to.
        /// </summary>
        public string PredictionId { get; set; }

        /// <summary>
        /// The input features that led to the prediction being reviewed.
        /// This helps in correlating feedback with specific data instances.
        /// </summary>
        [Required]
        public Dictionary<string, object> InputFeatures { get; set; }

        /// <summary>
        /// The actual outcome or corrected values as provided by the user.
        /// This is crucial for supervised learning or model retraining.
        /// </summary>
        [Required]
        public Dictionary<string, object> ActualOutcome { get; set; } // Can be complex object

        /// <summary>
        /// Optional: Free-text comments or explanations from the user.
        /// </summary>
        [StringLength(1000)]
        public string FeedbackText { get; set; }

        /// <summary>
        /// Optional: A simple flag indicating if the user considers the original prediction correct.
        /// </summary>
        public bool? IsCorrectPrediction { get; set; }

        /// <summary>
        /// Optional: Identifier of the user providing the feedback.
        /// </summary>
        public string UserId { get; set; }
        
        public ModelFeedbackRequestDto()
        {
            InputFeatures = new Dictionary<string, object>();
            ActualOutcome = new Dictionary<string, object>();
        }
    }
}