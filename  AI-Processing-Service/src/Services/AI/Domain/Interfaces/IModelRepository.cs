using AIService.Domain.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Domain.Interfaces
{
    /// <summary>
    /// Defines the contract for persisting and retrieving AiModel entities and their
    /// associated artifacts (like model files). This will be implemented by an adapter
    /// to REPO-DATA-SERVICE.
    /// </summary>
    public interface IModelRepository
    {
        /// <summary>
        /// Retrieves an AI model's metadata by its ID.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The AiModel entity if found; otherwise, null.</returns>
        Task<AiModel?> GetModelAsync(string modelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an AI model's metadata by its name and version.
        /// </summary>
        /// <param name="modelName">The name of the model.</param>
        /// <param name="version">The version of the model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The AiModel entity if found; otherwise, null.</returns>
        Task<AiModel?> GetModelByNameAndVersionAsync(string modelName, string version, CancellationToken cancellationToken = default);

        /// <summary>
        /// Saves (creates or updates) an AI model's metadata.
        /// </summary>
        /// <param name="model">The AiModel entity to save.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SaveModelAsync(AiModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the artifact (binary file) stream for a given model ID and version.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="version">The version of the model artifact.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A stream containing the model artifact if found; otherwise, null.</returns>
        Task<Stream?> GetModelArtifactStreamAsync(string modelId, string version, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores the model artifact (binary file) stream.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model.</param>
        /// <param name="version">The version of the model artifact.</param>
        /// <param name="artifactStream">The stream containing the model artifact.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task StoreModelArtifactAsync(string modelId, string version, Stream artifactStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a model and its associated artifacts.
        /// </summary>
        /// <param name="modelId">The unique identifier of the model to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if deletion was successful, false otherwise.</returns>
        Task<bool> DeleteModelAsync(string modelId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lists all AI models metadata, possibly with pagination in a real implementation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A collection of AiModel entities.</returns>
        Task<IEnumerable<AiModel>> ListModelsAsync(CancellationToken cancellationToken = default);
    }
}