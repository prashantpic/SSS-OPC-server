using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.System.Infrastructure.Data.Abstractions
{
    /// <summary>
    /// A contract for interacting with a blob storage solution to manage large, unstructured AI-related files.
    /// </summary>
    public interface IAiArtifactRepository
    {
        /// <summary>
        /// Uploads a file stream as an artifact to blob storage.
        /// </summary>
        /// <param name="artifactName">The unique name (key) of the artifact.</param>
        /// <param name="content">The stream containing the artifact's content.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The URI of the uploaded artifact.</returns>
        Task<Uri> UploadArtifactAsync(string artifactName, Stream content, CancellationToken cancellationToken);

        /// <summary>
        /// Downloads an artifact as a readable stream.
        /// </summary>
        /// <param name="artifactName">The unique name (key) of the artifact.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A stream containing the artifact's content.</returns>
        Task<Stream> DownloadArtifactAsync(string artifactName, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an artifact from blob storage.
        /// </summary>
        /// <param name="artifactName">The unique name (key) of the artifact.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteArtifactAsync(string artifactName, CancellationToken cancellationToken);
    }
}