using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Enums;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;

/// <summary>
/// Defines the contract for a data lifecycle strategy, allowing for different
/// implementations for various data types (e.g., historical, alarm, audit).
/// This enables the Strategy Pattern to manage data lifecycle policies.
/// </summary>
public interface IDataLifecycleStrategy
{
    /// <summary>
    /// Gets the specific data type this strategy is responsible for.
    /// </summary>
    DataType DataType { get; }

    /// <summary>
    /// Executes the lifecycle logic (archival, purging) for a given policy.
    /// </summary>
    /// <param name="policy">The data retention policy to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(DataRetentionPolicy policy, CancellationToken cancellationToken);
}