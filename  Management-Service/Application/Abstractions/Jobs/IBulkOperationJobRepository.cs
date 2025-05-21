using ManagementService.Domain.Aggregates.BulkOperationJobAggregate;
using ManagementService.Domain.SeedWork;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Application.Abstractions.Jobs;

// Repository interface for managing BulkOperationJob aggregate data.
public interface IBulkOperationJobRepository : IRepository<BulkOperationJob>
{
    /// <summary>
    /// Gets a BulkOperationJob by its unique identifier.
    /// </summary>
    /// <param name="id">The job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The BulkOperationJob or null if not found.</returns>
    Task<BulkOperationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new BulkOperationJob to the repository.
    /// </summary>
    /// <param name="job">The job to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(BulkOperationJob job, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing BulkOperationJob in the repository.
    /// </summary>
    /// <param name="job">The job to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(BulkOperationJob job, CancellationToken cancellationToken);
}