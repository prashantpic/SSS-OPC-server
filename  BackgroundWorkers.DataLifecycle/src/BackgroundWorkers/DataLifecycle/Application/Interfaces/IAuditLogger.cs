using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;

/// <summary>
/// Defines the contract for a service that logs auditable events. This decouples
/// application logic from how and where audit trails are stored (e.g., message queue,
/// dedicated API, database).
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs a significant data management action, such as archiving and purging, to a central audit trail.
    /// </summary>
    /// <param name="action">The action being performed (e.g., "Archive", "Purge").</param>
    /// <param name="dataType">The type of data being affected.</param>
    /// <param name="success">Whether the action was successful.</param>
    /// <param name="recordsAffected">The number of records affected by the action.</param>
    /// <param name="details">Optional details, such as error messages or file locations.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous logging operation.</returns>
    Task LogDataLifecycleEventAsync(
        string action,
        DataType dataType,
        bool success,
        long recordsAffected,
        string? details = null,
        CancellationToken cancellationToken = default);
}