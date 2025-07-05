using AiService.Application.Interfaces.Infrastructure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace AiService.Infrastructure.Storage;

public class BlobModelArtifactStorage : IModelArtifactStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobModelArtifactStorage> _logger;
    private const string ContainerName = "aimodels";

    public BlobModelArtifactStorage(BlobServiceClient blobServiceClient, ILogger<BlobModelArtifactStorage> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    private async Task<BlobContainerClient> GetContainerClient()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        return containerClient;
    }

    public async Task<Stream> GetModelStreamAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClient();
            var blobClient = containerClient.GetBlobClient(storagePath);
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download model from storage path {StoragePath}", storagePath);
            throw;
        }
    }

    public async Task<string> UploadModelAsync(Stream modelStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var containerClient = await GetContainerClient();
            var blobName = $"{Guid.NewGuid()}-{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(modelStream, new BlobHttpHeaders { ContentType = "application/octet-stream" }, cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully uploaded model {FileName} to blob storage as {BlobName}", fileName, blobName);
            return blobName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload model {FileName} to blob storage", fileName);
            throw;
        }
    }
}