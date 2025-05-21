namespace AIService.Domain.Interfaces
{
    using AIService.Domain.Models;
    using System.IO;
    using System.Threading.Tasks;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the contract for persisting and retrieving AiModel entities
    /// and their associated artifacts (like model files).
    /// This interface will be implemented by an adapter to REPO-DATA-SERVICE.
    /// (REQ-7-006)
    /// </summary>
    public interface IModelRepository
    {
        /// <summary>
        /// Retrieves an AiModel entity by its unique identifier and version.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="version">The specific version of the model. If null, the latest or default version might be retrieved.</param>
        /// <returns>The AiModel entity if found; otherwise, null.</returns>
        Task<AiModel> GetModelAsync(string modelId, string version = null);

        /// <summary>
        /// Retrieves all versions of an AiModel entity by its unique identifier.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <returns>A list of AiModel entities for the given ID, or an empty list if none are found.</returns>
        Task<IEnumerable<AiModel>> GetModelVersionsAsync(string modelId);

        /// <summary>
        /// Retrieves all AiModel entities, optionally filtered by type or other criteria.
        /// </summary>
        /// <returns>A list of AiModel entities.</returns>
        Task<IEnumerable<AiModel>> GetAllModelsAsync(); // Add filtering parameters as needed

        /// <summary>
        /// Saves (creates or updates) an AiModel entity.
        /// </summary>
        /// <param name="model">The AiModel entity to save.</param>
        /// <returns>The saved AiModel entity, possibly with updated fields (e.g., ID, timestamp).</returns>
        Task<AiModel> SaveModelAsync(AiModel model);

        /// <summary>
        /// Deletes an AiModel entity by its unique identifier and version.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model to delete.</param>
        /// <param name="version">The specific version of the model to delete.</param>
        /// <returns>True if deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteModelAsync(string modelId, string version);
        
        /// <summary>
        /// Retrieves the artifact (e.g., model file) for a given AiModel as a stream.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="version">The specific version of the model artifact to retrieve.</param>
        /// <returns>A stream containing the model artifact if found; otherwise, null.</returns>
        Task<Stream> GetModelArtifactStreamAsync(string modelId, string version);

        /// <summary>
        /// Saves the artifact (e.g., model file) for a given AiModel.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="version">The specific version of the model artifact.</param>
        /// <param name="artifactStream">A stream containing the model artifact.</param>
        /// <param name="storageReference">A reference string (e.g. file name or path within blob storage) for the artifact.</param>
        /// <returns>A string representing the storage reference or path where the artifact was saved.</returns>
        Task<string> SaveModelArtifactAsync(string modelId, string version, Stream artifactStream, string storageReference);
    }
}