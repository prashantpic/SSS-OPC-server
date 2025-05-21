using MediatR;
using System.Collections.Generic;

namespace AIService.Application.ModelManagement.Commands
{
    /// <summary>
    /// Encapsulates user feedback (validation, correction) for a specific model prediction,
    /// to be processed by the ModelManagementAppService.
    /// REQ-7-005: Model Feedback Loop
    /// REQ-7-011: Anomaly Labeling (can be a form of feedback)
    /// </summary>
    public class RegisterModelFeedbackCommand : IRequest<Unit> // Unit if no specific result, or a FeedbackAcknowledgement
    {
        /// <summary>
        /// Identifier of the model for which feedback is provided.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Version of the model.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Optional unique identifier for the specific prediction instance, if available.
        /// </summary>
        public string PredictionId { get; set; }

        /// <summary>
        /// A snapshot of the input data that led to the prediction.
        /// Could be a JSON string or a dictionary.
        /// </summary>
        public Dictionary<string, object> InputDataSnapshot { get; set; }

        /// <summary>
        /// The original output/prediction from the model.
        /// Could be a JSON string or a dictionary.
        /// </summary>
        public Dictionary<string, object> OriginalOutput { get; set; }

        /// <summary>
        /// Type of feedback (e.g., "Correct", "Incorrect", "Misclassified", "FalsePositive", "FalseNegative", "CorrectedLabel").
        /// </summary>
        public string FeedbackType { get; set; }

        /// <summary>
        /// If feedback involves correction, this holds the corrected output/label.
        /// Could be a JSON string or a dictionary.
        /// </summary>
        public Dictionary<string, object> CorrectedOutput { get; set; }

        /// <summary>
        /// Additional textual comments from the user.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Identifier of the user providing the feedback.
        /// </summary>
        public string UserId { get; set; }

        public RegisterModelFeedbackCommand()
        {
            InputDataSnapshot = new Dictionary<string, object>();
            OriginalOutput = new Dictionary<string, object>();
            CorrectedOutput = new Dictionary<string, object>();
        }
    }

    // Example for acknowledgement, if needed
    // public class FeedbackAcknowledgement
    // {
    //     public bool Success { get; set; }
    //     public string Message { get; set; }
    //     public string FeedbackId { get; set; } // If feedback is stored and gets an ID
    // }
}