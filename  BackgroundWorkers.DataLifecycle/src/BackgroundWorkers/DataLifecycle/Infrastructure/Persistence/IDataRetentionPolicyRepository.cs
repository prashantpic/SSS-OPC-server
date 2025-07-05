using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Persistence;

/// <summary>
/// Defines the data access contract for DataRetentionPolicy entities.
/// This abstracts the database operations for retrieving data retention policy configurations.
/// </summary>
public interface IDataRetentionPolicyRepository
{
    /// <summary>
    /// Retrieves all data retention policies that are marked as active.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of active policies.</returns>
    Task<IEnumerable<DataRetentionPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken);
}