using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for managing edge AI functionalities, 
    /// including model execution, input/output handling, and model updates.
    /// </summary>
    public interface IEdgeIntelligenceService
    {
        /// <summary>
        /// Loads an AI model for execution.
        /// </summary>
        /// <param name="modelName">The name of the model.</param>
        /// <param name="version">The version of the model.</param>
        /// <returns>A task representing the asynchronous operation. True if successful, false otherwise.</returns>
        Task<bool> LoadModelAsync(string modelName, string version);

        /// <summary>
        /// Executes a loaded AI model with the given input.
        /// </summary>
        /// <param name="modelName">The name of the model to execute.</param>
        /// <param name="input">The input data for the model.</param>
        /// <returns>The output from the AI model, or null if execution failed.</returns>
        Task<EdgeModelOutputDto?> ExecuteModelAsync(string modelName, EdgeModelInputDto input);

        /// <summary>
        /// Handles updates to an AI model, such as deploying a new version.
        /// </summary>
        /// <param name="metadata">The metadata of the new model.</param>
        /// <param name="modelBytes">The byte content of the new model file.</param>
        /// <returns>A task representing the asynchronous operation. True if update was successful, false otherwise.</returns>
        Task<bool> HandleModelUpdateAsync(EdgeModelMetadataDto metadata, byte[] modelBytes);

        /// <summary>
        /// Gets the status of a specific AI model.
        /// </summary>
        /// <param name="modelName">The name of the model.</param>
        /// <returns>A DTO representing the model's status, or null if not found.</returns>
        Task<EdgeModelMetadataDto?> GetModelStatusAsync(string modelName); // Using EdgeModelMetadataDto as a status representation for now
    }
}