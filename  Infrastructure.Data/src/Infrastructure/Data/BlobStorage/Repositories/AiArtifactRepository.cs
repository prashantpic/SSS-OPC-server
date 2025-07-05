using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using Opc.System.Infrastructure.Data.Abstractions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#region Placeholder Config
public class BlobStorageOptions
{
    public string ContainerName { get; set; } = string.Empty;
}
#endregion

namespace Opc.System.Infrastructure.Data.BlobStorage.Repositories
{
    /// <summary>
    /// Provides a concrete implementation for storing and retrieving AI artifacts from a configured Azure Blob Storage container.
    /// </summary>
    public class AiArtifactRepository : IAiArtifactRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AiArtifactRepository(BlobServiceClient blobServiceClient, IOptions<BlobStorageOptions> options)
        {
            _blobServiceClient = blobServiceClient;
            _containerName = options.Value.ContainerName ?? throw new ArgumentNullException(nameof(options.Value.ContainerName));
        }

        /// <summary>
        /// Deletes an artifact from blob storage if it exists.
        /// </summary>
        /// <param name="artifactName">The unique name (key) of the artifact.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        public async Task DeleteArtifactAsync(string artifactName, CancellationToken cancellationToken)
        {
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(artifactName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Downloads an artifact as a readable stream.
        /// </summary>
        /// <param name="artifactName">The unique name (key) of the artifact.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A stream containing the artifact's content.</returns>
        public async Task<Stream> DownloadArtifactAsync(string artifactName, CancellationToken cancellationToken)
        {
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(artifactName);
            return await blobClient.OpenReadAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Uploads a file stream as an artifact to blob storage.
        /// </summary>
        /// <param name="artifactName">The unique name (key) of the artifact.</param>
        /// <param name="content">The stream containing the artifact's content.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The URI of the uploaded artifact.</returns>
        public async Task<Uri> UploadArtifactAsync(string artifactName, Stream content, CancellationToken cancellationToken)
        {
            var containerClient = await GetContainerClientAsync(cancellationToken);
            var blobClient = containerClient.GetBlobClient(artifactName);
            await blobClient.UploadAsync(content, overwrite: true, cancellationToken);
            return blobClient.Uri;
        }

        private async Task<BlobContainerClient> GetContainerClientAsync(CancellationToken cancellationToken)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
            return containerClient;
        }
    }
}