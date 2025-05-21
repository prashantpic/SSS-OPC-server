namespace AIService.Domain.Interfaces
{
    using AIService.Domain.Models;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for interacting with an MLOps platform.
    /// Covers functionalities like model registration, deployment tracking, and performance monitoring.
    /// (REQ-7-004, REQ-7-010, REQ-8-001)
    /// </summary>
    public interface IMlLopsClient
    {
        /// <summary>
        /// Registers a new model or a new version of an existing model with the MLOps platform.
        /// </summary>
        /// <param name="model">The AiModel metadata.</param>
        /// <param name="artifactStream">A stream containing the model artifact file.</param>
        /// <returns>True if registration was successful; otherwise, false. May also return a platform-specific model ID or version.</returns>
        Task<bool> RegisterModelAsync(AiModel model, Stream artifactStream);

        /// <summary>
        /// Logs prediction feedback to the MLOps platform for monitoring and retraining purposes.
        /// (REQ-7-005)
        /// </summary>
        /// <param name="feedback">The model feedback details.</param>
        /// <returns>True if feedback logging was successful; otherwise, false.</returns>
        Task<bool> LogPredictionFeedbackAsync(ModelFeedback feedback);

        /// <summary>
        /// Retrieves the deployment status of a specific model in a given environment from the MLOps platform.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="modelVersion">The version of the model.</param>
        /// <param name="environment">The target deployment environment (e.g., "production", "edge-group-A").</param>
        /// <returns>A string or object representing the deployment status.</returns>
        Task<string> GetModelDeploymentStatusAsync(string modelId, string modelVersion, string environment);

        /// <summary>
        /// Initiates the deployment of a specific model version to designated edge devices via the MLOps platform.
        /// (REQ-8-001)
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="modelVersion">The version of the model to deploy.</param>
        /// <param name="deviceIds">A collection of unique identifiers for the target edge devices.</param>
        /// <returns>True if the deployment process was successfully initiated; otherwise, false. May also return a deployment job ID.</returns>
        Task<bool> TriggerEdgeDeploymentAsync(string modelId, string modelVersion, IEnumerable<string> deviceIds);

        /// <summary>
        /// Logs performance metrics for a model to the MLOps platform.
        /// (REQ-7-010)
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="modelVersion">The version of the model.</param>
        /// <param name="metrics">A dictionary of metric names and their values.</param>
        /// <returns>True if metrics logging was successful; otherwise, false.</returns>
        Task<bool> LogModelMetricsAsync(string modelId, string modelVersion, Dictionary<string, double> metrics);

         /// <summary>
        /// Initiates a retraining workflow for a model on the MLOps platform.
        /// </summary>
        /// <param name="modelId">The ID of the model to retrain.</param>
        /// <param name="modelVersion">Optional specific version to base retraining on.</param>
        /// <param name="retrainingParameters">Optional parameters for the retraining job.</param>
        /// <returns>True if the retraining workflow was successfully initiated.</returns>
        Task<bool> InitiateRetrainingWorkflowAsync(string modelId, string modelVersion = null, Dictionary<string, string> retrainingParameters = null);
    }

    /// <summary>
    /// Represents feedback on a model's prediction. (REQ-7-005)
    /// </summary>
    public class ModelFeedback
    {
        public string ModelId { get; set; }
        public string ModelVersion { get; set; }
        public ModelInput PredictionInput { get; set; } // The input that led to the prediction
        public ModelOutput OriginalPrediction { get; set; } // The model's original output
        public object ActualOutcome { get; set; } // The ground truth or corrected outcome (can be structured)
        public string FeedbackNotes { get; set; } // User comments or reasons for feedback
        public string UserId { get; set; } // User providing the feedback
        public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;
        public Dictionary<string, string> AdditionalMetadata { get; set; } // For extensibility

        public ModelFeedback()
        {
            AdditionalMetadata = new Dictionary<string, string>();
        }
    }
}