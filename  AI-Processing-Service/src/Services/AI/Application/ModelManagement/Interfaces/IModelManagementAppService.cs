using AIService.Application.ModelManagement.Commands;
using AIService.Application.ModelManagement.Models; // Assuming this namespace for custom models
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
        /// Uploads a new AI model artifact and its metadata.
        /// </summary>
        /// <param name="request">The model upload request details.</param>
        /// <returns>Result of the upload operation.</returns>
        Task<ModelUploadResult> UploadModelAsync(ModelUploadRequest request);

        /// <summary>
        /// Registers user feedback for a specific model's prediction.
        /// </summary>
        /// <param name="feedbackCommand">The command containing feedback details.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task RegisterModelFeedbackAsync(RegisterModelFeedbackCommand feedbackCommand);

        /// <summary>
        /// Retrieves the status of a specific AI model.
        /// </summary>
        /// <param name="modelId">The ID of the model.</param>
        /// <param name="version">The version of the model.</param>
        /// <returns>Information about the model's status.</returns>
        Task<ModelStatusInfo> GetModelStatusAsync(string modelId, string version);

        /// <summary>
        /// Initiates a retraining workflow for a specified AI model.
        /// </summary>
        /// <param name="modelId">The ID of the model to retrain.</param>
        /// <param name="version">The version of the model to retrain (or base for new version).</param>
        /// <param name="parameters">Parameters for the retraining process.</param>
        /// <returns>Information about the initiated retraining task.</returns>
        Task<RetrainingWorkflowInfo> InitiateRetrainingWorkflowAsync(string modelId, string version, RetrainingParameters parameters);
    }
}