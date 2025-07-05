using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;

/// <summary>
/// Defines the contract for a service that retrieves data retention policies from a persistent store.
/// This abstracts the source of data retention policies, decoupling the core job logic
/// from the data access implementation.
/// </summary>
public interface IPolicyProvider
{
    /// <summary>
    /// Retrieves all currently active data retention policies that the lifecycle job needs to enforce.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of active data retention policies.</returns>
    Task<IEnumerable<DataRetentionPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken);
}