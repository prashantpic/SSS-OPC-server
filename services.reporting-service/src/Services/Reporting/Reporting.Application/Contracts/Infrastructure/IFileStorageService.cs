namespace Reporting.Application.Contracts.Infrastructure;

/// <summary>
/// Interface for a service that can store file content.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves file content to a storage system.
    /// </summary>
    /// <param name="content">The byte content of the file.</param>
    /// <param name="fileName">The desired name of the file.</param>
    /// <param name="container">A logical container or folder for the file (e.g., "generated-reports").</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The full path or URL to the saved file.</returns>
    Task<string> SaveAsync(byte[] content, string fileName, string container, CancellationToken cancellationToken = default);
}