using AIService.Application.ModelManagement.Commands;
using AIService.Application.ModelManagement.Models.Results; // Placeholder for result models
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // For IEnumerable

namespace AIService.Application.ModelManagement.Interfaces
{
    /// <summary>
    /// Defines contracts for application-level model management tasks such as
    /// uploading models, registering feedback, retrieving model status, and initiating retraining.
    /// REQ-7-004: MLOps Integration.
    /// REQ-7-005: Model Feedback Loop.
    /// REQ-7-010: AI Model Performance Monitoring (partially, by getting status/triggering retraining).
    /// </summary>
    public interface IModelManagementAppService
    {
        /// <summary>
        /// Uploads a new AI model artifact and its metadata.
        /// </summary>
        /// <param name="modelName">The name of the model.</param>
        /// <param name="modelVersion">The version of the model.</param>
        /// <param name="modelType">The type of the model (e.g., PredictiveMaintenance, AnomalyDetection).</param>
        /// <param name="modelFormat">The format of the model (e.g., ONNX, MLNetZip).</param>
        /// <param name="modelFileStream">The stream containing the model artifact.</param>
        /// <param name="description">Optional description of the model.</param>
        /// <param name="inputSchema">Optional JSON string defining the model's input schema.</param>
        /// <param name="outputSchema">Optional JSON string defining the model's output schema.</param>
        /// <param name="tags">Optional dictionary of tags for the model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result object indicating the success of the upload and the new model's ID.</returns>
        Task<ModelUploadResult> UploadModelAsync(
            string modelName,
            string modelVersion,
            string modelType, // Consider using Domain.Enums.ModelType if stringly typing is an issue
            string modelFormat, // Consider using Domain.Enums.ModelFormat
            Stream modelFileStream,
            string? description,
            string? inputSchema,
            string? outputSchema,
            Dictionary<string, string>? tags,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Registers user feedback for a specific model's prediction.
        /// </summary>
        /// <param name="feedbackCommand">The command containing feedback details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<ModelOperationResult> RegisterFeedbackAsync(
            RegisterModelFeedbackCommand feedbackCommand,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the status of a specific AI model.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="modelVersion">Optional specific version of the model. If null, status for all versions or primary version might be returned.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Information about the model's status.</returns>
        Task<ModelStatusInfo> GetModelStatusAsync(
            string modelId,
            string? modelVersion = null,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a list of available models, potentially filtered.
        /// </summary>
        /// <param name="filterByName">Optional filter by model name.</param>
        /// <param name="filterByType">Optional filter by model type.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of model metadata.</returns>
        Task<IEnumerable<ModelMetadataResult>> ListModelsAsync(
            string? filterByName = null, 
            string? filterByType = null, 
            CancellationToken cancellationToken = default);


        /// <summary>
        /// Initiates a retraining workflow for a specified AI model.
        /// </summary>
        /// <param name="modelId">The ID of the model to be retrained.</param>
        /// <param name="retrainingParameters">Parameters specific to the retraining process.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result object indicating the initiation of the retraining workflow.</returns>
        Task<RetrainingInitiationResult> InitiateRetrainingWorkflowAsync(
            string modelId,
            Dictionary<string, object> retrainingParameters, // Simple representation for parameters
            CancellationToken cancellationToken = default);
    }
}