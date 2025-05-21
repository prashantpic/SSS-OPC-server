using MediatR;
using System;
using System.Collections.Generic;

namespace AIService.Application.ModelManagement.Commands
{
    /// <summary>
    /// Encapsulates user feedback (validation, correction) for a specific model prediction,
    /// to be processed by the ModelManagementAppService.
    /// REQ-7-005: Model Feedback Loop
    /// REQ-7-011: Anomaly Labeling
    /// </summary>
    public class RegisterModelFeedbackCommand : IRequest<Unit> // Unit indicates no specific return value
    {
        /// <summary>
        /// The ID of the model for which feedback is being provided.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the model.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Optional: An identifier for the specific prediction instance this feedback pertains to.
        /// </summary>
        public string PredictionId { get; set; }

        /// <summary>
        /// A snapshot of the input data that led to the prediction.
        /// Could be a JSON string or a structured object.
        /// </summary>
        public Dictionary<string, object> InputDataSnapshot { get; set; }

        /// <summary>
        /// The actual outcome or ground truth, as provided by the user.
        /// Could be a JSON string or a structured object.
        /// </summary>
        public Dictionary<string, object> ActualOutcome { get; set; }
        
        /// <summary>
        /// The predicted outcome by the model.
        /// Could be a JSON string or a structured object.
        /// </summary>
        public Dictionary<string, object> PredictedOutcome { get; set; }


        /// <summary>
        /// Indicates if the model's prediction was correct according to the user.
        /// Nullable if this is not a simple yes/no feedback.
        /// </summary>
        public bool? IsCorrect { get; set; }

        /// <summary>
        /// Additional textual notes or comments from the user.
        /// </summary>
        public string FeedbackNotes { get; set; }

        /// <summary>
        /// Timestamp of when the feedback was provided.
        /// </summary>
        public DateTime FeedbackTimestamp { get; set; }

        /// <summary>
        /// User ID of the person providing feedback.
        /// </summary>
        public string UserId { get; set; }

        public RegisterModelFeedbackCommand()
        {
            FeedbackTimestamp = DateTime.UtcNow;
            InputDataSnapshot = new Dictionary<string, object>();
            ActualOutcome = new Dictionary<string, object>();
            PredictedOutcome = new Dictionary<string, object>();
        }
    }
}