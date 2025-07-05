using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Opc.System.Services.AI.Application.Interfaces;

namespace Opc.System.Services.AI.Infrastructure.Stores;

/// <summary>
/// Configuration for Azure Blob Storage.
/// </summary>
public record AzureStorageConfig
{
    public string ModelContainerName { get; init; } = string.Empty;
}

/// <summary>
/// Implements the IModelStore interface using Azure Blob Storage for storing AI model artifacts.
/// </summary>
public class BlobModelStore : IModelStore
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureStorageConfig _config;

    public BlobModelStore(BlobServiceClient blobServiceClient, IOptions<AzureStorageConfig> config)
    {
        _blobServiceClient = blobServiceClient;
        _config = config.Value;
    }

    /// <inheritdoc />
    public async Task<string> SaveModelAsync(string modelId, string version, Stream modelStream, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(GetBlobName(modelId, version));

        await blobClient.UploadAsync(modelStream, new BlobHttpHeaders { ContentType = "application/octet-stream" }, cancellationToken: cancellationToken);

        return blobClient.Uri.ToString();
    }

    /// <inheritdoc />
    public async Task<Stream> GetModelStreamAsync(string modelId, string version, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(GetBlobName(modelId, version));

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            throw new FileNotFoundException($"Model artifact not found for model '{modelId}' version '{version}'.");
        }
        
        return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
    }
    
    /// <inheritdoc />
    public async Task DeleteModelAsync(string modelId, string version, CancellationToken cancellationToken = default)
    {
        var containerClient = await GetContainerClientAsync(cancellationToken);
        var blobClient = containerClient.GetBlobClient(GetBlobName(modelId, version));
        await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
    }

    private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ModelContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        return containerClient;
    }

    private static string GetBlobName(string modelId, string version) => $"{modelId}/{version}.onnx";
}