namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Provides a contract for interacting with a blob/file storage system for AI model artifacts.
/// This abstracts the underlying storage mechanism (e.g., local disk, cloud blob storage) for model files.
/// </summary>
public interface IModelArtifactStorage
{
    /// <summary>
    /// Uploads a model file from a stream to the artifact storage.
    /// </summary>
    /// <param name="modelStream">The stream containing the model file data.</param>
    /// <param name="fileName">The desired name for the file in the storage.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique storage path/identifier for the uploaded file.</returns>
    Task<string> UploadModelAsync(Stream modelStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a model file as a stream from the artifact storage.
    /// </summary>
    /// <param name="storagePath">The unique storage path/identifier of the file to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stream of the model file.</returns>
    Task<Stream> GetModelStreamAsync(string storagePath, CancellationToken cancellationToken = default);
}