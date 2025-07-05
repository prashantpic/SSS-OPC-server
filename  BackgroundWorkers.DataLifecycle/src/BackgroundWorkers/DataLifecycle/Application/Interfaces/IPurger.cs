using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;

/// <summary>
/// Represents the result of a purge operation.
/// </summary>
/// <param name="Success">Indicates whether the operation was successful.</param>
/// <param name="RecordsPurged">The number of records that were deleted.</param>
/// <param name="Details">Additional details about the operation's outcome.</param>
public record PurgeResult(bool Success, long RecordsPurged, string? Details);

/// <summary>
/// Defines the contract for a service that securely purges data from a source based on a time threshold.
/// This abstracts the mechanism of data deletion, allowing for different implementations
/// depending on the data source.
/// </summary>
public interface IPurger
{
    /// <summary>
    /// Securely and permanently deletes data of a specific type that is older than a given threshold.
    /// </summary>
    /// <param name="dataType">The type of data to purge.</param>
    /// <param name="threshold">The timestamp threshold; data older than this will be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result object containing the status and count of deleted records.</returns>
    Task<PurgeResult> PurgeAsync(DataType dataType, DateTimeOffset threshold, CancellationToken cancellationToken);
}