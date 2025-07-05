using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;

/// <summary>
/// Represents the result of an archival operation.
/// </summary>
/// <param name="Success">Indicates whether the operation was successful.</param>
/// <param name="RecordsArchived">The number of records that were archived.</param>
/// <param name="Details">Additional details about the operation's outcome.</param>
public record ArchiveResult(bool Success, long RecordsArchived, string? Details);

/// <summary>
/// Defines the contract for a service that archives data from a source to a destination.
/// This abstracts the mechanism of archiving data, allowing different implementations
/// for different storage backends (e.g., S3, Azure Blob, local file system).
/// </summary>
public interface IArchiver
{
    /// <summary>
    /// Archives data of a specific type that is older than a given threshold to a specified location.
    /// The implementation is responsible for fetching, serializing, and uploading the data.
    /// </summary>
    /// <param name="dataType">The type of data to archive.</param>
    /// <param name="threshold">The timestamp threshold; data older than this will be archived.</param>
    /// <param name="archiveLocation">The destination for the archive (e.g., container/bucket name).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result object containing the status and count of archived records.</returns>
    Task<ArchiveResult> ArchiveAsync(DataType dataType, DateTimeOffset threshold, string archiveLocation, CancellationToken cancellationToken);
}