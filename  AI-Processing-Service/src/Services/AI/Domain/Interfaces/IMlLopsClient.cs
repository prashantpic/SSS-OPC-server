using AIService.Domain.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Domain.Interfaces
{
    /// <summary>
    /// Defines the contract for interacting with an MLOps platform,
    /// covering functionalities like model registration, deployment tracking, and performance monitoring.
    /// Supports REQ-7-004.
    /// </summary>
    public interface IMlLopsClient
    {
        /// <summary>
        /// Registers a model with the MLOps platform.
        /// </summary>
        /// <param name="model">The AiModel metadata.</param>
        /// <param name="artifactStream">A stream containing the model artifact.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A unique identifier or version for the registered model from the MLOps platform, if applicable.</returns>
        Task<string?> RegisterModelAsync(AiModel model, Stream artifactStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Logs prediction feedback to the MLOps platform for monitoring or retraining purposes.
        /// </summary>
        /// <param name="modelId">The ID of the model used for prediction.</param>
        /// <param name="modelVersion">The version of the model used.</param>
        /// <param name="input">The input data provided to the model.</param>
        /// <param name="predictedOutput">The output predicted by the model.</param>
        /// <param name="actualOutput">The actual (ground truth) output, if available.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task LogPredictionFeedbackAsync(
            string modelId,
            string modelVersion,
            ModelInput input,
            ModelOutput predictedOutput,
            ModelOutput? actualOutput = null, // Actual output might not always be available immediately
            Dictionary<string, string>? customTags = null, // Additional tags for MLOps
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the deployment status of a model in a specific environment from the MLOps platform.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="environment">The target deployment environment (e.g., "staging", "production", "edge-device-group-X").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A string representing the deployment status, or null if not found/applicable.</returns>
        Task<string?> GetModelDeploymentStatusAsync(string modelId, string environment, CancellationToken cancellationToken = default);

        /// <summary>
        /// Triggers the deployment of a specific model version to designated edge devices or environments via the MLOps platform.
        /// </summary>
        /// <param name="modelId">The ID of the model to deploy.</param>
        /// <param name="modelVersion">The version of the model to deploy.</param>
        /// <param name="targetIdentifiers">A collection of identifiers for target devices or deployment environments.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A tracking ID or status message for the deployment operation.</returns>
        Task<string?> TriggerDeploymentAsync(string modelId, string modelVersion, IEnumerable<string> targetIdentifiers, CancellationToken cancellationToken = default);


        /// <summary>
        /// Retrieves a list of registered models from the MLOps platform.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of AiModel metadata obtained from the MLOps platform.</returns>
        Task<IEnumerable<AiModel>> GetRegisteredModelsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves detailed information for a specific model (and optionally version) from the MLOps platform.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="version">Optional. The specific version of the model. If null, latest or all versions might be considered by implementation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>AiModel metadata if found; otherwise, null.</returns>
        Task<AiModel?> GetModelDetailsAsync(string modelId, string? version = null, CancellationToken cancellationToken = default);

         /// <summary>
        /// Initiates a retraining workflow for a given model.
        /// </summary>
        /// <param name="modelId">The ID of the model to retrain.</param>
        /// <param name="retrainingParameters">Optional parameters for the retraining job.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An identifier for the retraining job or status.</returns>
        Task<string?> InitiateRetrainingWorkflowAsync(string modelId, Dictionary<string, string>? retrainingParameters = null, CancellationToken cancellationToken = default);
    }
}