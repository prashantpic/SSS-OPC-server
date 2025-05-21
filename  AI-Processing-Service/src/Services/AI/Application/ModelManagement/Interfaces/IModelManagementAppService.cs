using AIService.Application.ModelManagement.Commands;
using AIService.Domain.Enums; // For ModelType, ModelFormat
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AIService.Application.ModelManagement.Interfaces
{
    /// <summary>
    /// Defines contracts for application-level model management tasks like deploying models
    /// to the server-side, handling feedback, and initiating retraining workflows.
    /// REQ-7-004: MLOps Integration
    /// REQ-7-005: Model Feedback Loop
    /// REQ-7-010: AI Model Performance Monitoring
    /// </summary>
    public interface IModelManagementAppService
    {
        /// <summary>
        /// Uploads a model artifact, registers its metadata, and integrates with MLOps.
        /// </summary>
        /// <param name="modelArtifactStream">Stream of the model artifact.</param>
        /// <param name="originalFileName">Original name of the uploaded file.</param>
        /// <param name="modelName">User-defined name for the model.</param>
        /// <param name="version">Version string for the model.</param>
        /// <param name="description">Description of the model.</param>
        /// <param name="modelType">Type of the model (e.g., PredictiveMaintenance, AnomalyDetection).</param>
        /// <param name="modelFormat">Format of the model (e.g., ONNX, TensorFlowLite).</param>
        /// <param name="inputSchemaJson">JSON string defining the model's input schema. REQ-7-002, REQ-7-009</param>
        /// <param name="outputSchemaJson">JSON string defining the model's output schema.</param>
        /// <param name="tags">Additional tags for MLOps (e.g., experiment ID, author).</param>
        /// <returns>The unique ID of the registered AiModel.</returns>
        Task<string> UploadAndRegisterModelAsync(
            Stream modelArtifactStream,
            string originalFileName,
            string modelName,
            string version,
            string description,
            ModelType modelType,
            ModelFormat modelFormat,
            string inputSchemaJson,
            string outputSchemaJson,
            Dictionary<string, string> tags);

        /// <summary>
        /// Processes user feedback for a given model prediction.
        /// </summary>
        /// <param name="feedbackCommand">Command containing feedback details.</param>
        Task ProcessModelFeedbackAsync(RegisterModelFeedbackCommand feedbackCommand);

        /// <summary>
        /// Retrieves the status of a model (e.g., from MLOps or internal state).
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="version">The version of the model.</param>
        /// <returns>An object representing the model's status. // TODO: Define a specific ModelStatusDto.</returns>
        Task<object> GetModelStatusAsync(string modelId, string version);

        /// <summary>
        /// Initiates a retraining workflow for a specified model.
        /// </summary>
        /// <param name="modelId">The ID of the model to retrain.</param>
        /// <param name="version">The version of the model to base retraining on (optional).</param>
        /// <param name="retrainingParams">Parameters for the retraining process.</param>
        Task InitiateRetrainingWorkflowAsync(string modelId, string version, Dictionary<string, object> retrainingParams);

        /// <summary>
        /// Retrieves a list of available models, possibly with filtering/pagination.
        /// </summary>
        /// <returns>A list of model metadata objects. // TODO: Define a specific ModelMetadataDto.</returns>
        Task<IEnumerable<object>> ListModelsAsync(/* TODO: Add filter/paging parameters */);

        /// <summary>
        /// Retrieves details for a specific model.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="version">The version of the model.</param>
        /// <returns>A model detail object. // TODO: Define a specific ModelDetailDto.</returns>
        Task<object> GetModelDetailsAsync(string modelId, string version);
    }
}