namespace Opc.System.Services.AI.Application.Interfaces;

/// <summary>
/// Defines the contract for storing and retrieving AI model binary artifacts.
/// This provides an abstraction for the physical storage of AI model files (e.g., ONNX files),
/// decoupling the application logic from the specific storage technology.
/// </summary>
public interface IModelStore
{
    /// <summary>
    /// Saves a model artifact from a stream to the underlying storage.
    /// </summary>
    /// <param name="modelId">The unique identifier for the model.</param>
    /// <param name="version">The version tag for the model artifact.</param>
    /// <param name="modelStream">The stream containing the model binary data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The URI or path to the stored model artifact.</returns>
    Task<string> SaveModelAsync(string modelId, string version, Stream modelStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a model artifact as a stream from the underlying storage.
    /// </summary>
    /// <param name="modelId">The unique identifier for the model.</param>
    /// <param name="version">The version tag for the model artifact.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A stream containing the model binary data.</returns>
    Task<Stream> GetModelStreamAsync(string modelId, string version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a model artifact from the underlying storage.
    /// </summary>
    /// <param name="modelId">The unique identifier for the model.</param>
    /// <param name="version">The version tag for the model artifact.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteModelAsync(string modelId, string version, CancellationToken cancellationToken = default);
}

// NOTE: The following interfaces are defined here to satisfy dependencies from command handlers
// without creating unlisted files. Ideally, each would be in its own file.

/// <summary>
/// Defines a contract for a client that interacts with the Data Service.
/// This would typically be a gRPC client.
/// </summary>
public interface IDataServiceClient
{
    Task<IEnumerable<object>> GetHistoricalDataForAssetAsync(Guid assetId, CancellationToken cancellationToken);
    Task<object?> GetLatestValueAsync(string tagId, CancellationToken cancellationToken);
    Task<IEnumerable<object>> GetHistoricalDataAsync(string tagId, DateTime start, DateTime end, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a contract for a repository that manages Natural Language Query aliases.
/// </summary>
public interface INlqAliasRepository
{
    Task<string?> ResolveAliasAsync(string alias, CancellationToken cancellationToken);
}