using MediatR; // Not strictly a MediatR command if handled directly by AppService, but good for consistency if it were.
using System.Collections.Generic;
using System;
using AIService.Application.ModelManagement.Models.Results; // For ModelOperationResult

namespace AIService.Application.ModelManagement.Commands
{
    /// <summary>
    /// Encapsulates user feedback for a specific model prediction.
    /// This can be used for model monitoring, retraining data collection, and improving model performance.
    /// REQ-7-005: Model Feedback Loop.
    /// REQ-7-011: Anomaly Labeling (a form of feedback).
    /// </summary>
    public class RegisterModelFeedbackCommand // : IRequest<ModelOperationResult> // If handled by MediatR handler
    {
        /// <summary>
        /// The unique identifier of the model for which feedback is being provided.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// The version of the model used for the prediction.
        /// </summary>
        public string ModelVersion { get; set; }

        /// <summary>
        /// Optional: A unique identifier for the specific prediction instance, if available.
        /// </summary>
        public string? PredictionId { get; set; }

        /// <summary>
        /// The input features that led to the prediction.
        /// </summary>
        public Dictionary<string, object> InputFeatures { get; set; }

        /// <summary>
        /// The original output/prediction provided by the model.
        /// </summary>
        public Dictionary<string, object> ModelOutput { get; set; }

        /// <summary>
        /// The actual/correct output or label provided by the user or an oracle.
        /// </summary>
        public Dictionary<string, object> ActualOutput { get; set; }

        /// <summary>
        /// Optional: A flag indicating if the user considers the model's prediction correct.
        /// If null, correctness might be inferred from comparison of ModelOutput and ActualOutput.
        /// </summary>
        public bool? IsCorrect { get; set; }

        /// <summary>
        /// Optional: Any textual comments or annotations from the user.
        /// </summary>
        public string? UserComments { get; set; }

        /// <summary>
        /// Optional: Timestamp when the feedback was provided by the user. 
        /// If null, will be set to current time upon processing.
        /// </summary>
        public DateTimeOffset? UserFeedbackTimestamp { get; set; }

        /// <summary>
        /// Optional: Identifier of the user providing the feedback.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Optional: Additional metadata related to the feedback.
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }


        public RegisterModelFeedbackCommand(
            string modelId,
            string modelVersion,
            Dictionary<string, object> inputFeatures,
            Dictionary<string, object> modelOutput,
            Dictionary<string, object> actualOutput)
        {
            ModelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
            ModelVersion = modelVersion ?? throw new ArgumentNullException(nameof(modelVersion));
            InputFeatures = inputFeatures ?? throw new ArgumentNullException(nameof(inputFeatures));
            ModelOutput = modelOutput ?? throw new ArgumentNullException(nameof(modelOutput));
            ActualOutput = actualOutput ?? throw new ArgumentNullException(nameof(actualOutput));
            Metadata = new Dictionary<string, string>();
        }
    }
}