using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.ModelManagement
{
    /// <summary>
    /// Data transfer object for users to submit feedback (validation, correction, comments)
    /// on predictions made by AI models.
    /// Used for REQ-7-005 and REQ-7-011.
    /// </summary>
    public class ModelFeedbackRequestDto
    {
        /// <summary>
        /// The ID of the model for which feedback is being provided.
        /// </summary>
        [Required]
        public string ModelId { get; set; }

        /// <summary>
        /// Optional: The ID of the specific prediction instance this feedback pertains to.
        /// </summary>
        public string PredictionId { get; set; }

        /// <summary>
        /// The input data that led to the prediction being reviewed.
        /// This helps in understanding the context of the feedback.
        /// </summary>
        [Required]
        public Dictionary<string, object> InputDataContext { get; set; }

        /// <summary>
        /// The actual outcome or corrected label provided by the user.
        /// For classification, this might be the correct class label.
        /// For regression, this might be the correct numerical value.
        /// For anomaly detection, this could be a boolean (true_anomaly/false_positive) or a label.
        /// </summary>
        [Required]
        public object ActualOutcome { get; set; } // Can be string, number, bool, or Dictionary<string, object>

        /// <summary>
        /// Optional: Free-text comments or notes from the user regarding this feedback.
        /// </summary>
        [StringLength(1000)]
        public string FeedbackText { get; set; }

        /// <summary>
        /// Optional: Identifier for the user providing the feedback.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Timestamp of when the feedback was submitted.
        /// If not provided, will be set by the server.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }
    }
}