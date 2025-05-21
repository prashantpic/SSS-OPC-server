using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;
using ManagementService.Domain.SeedWork;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Application.Abstractions.Jobs;

// Repository interface for managing ConfigurationMigrationJob aggregate data.
public interface IConfigurationMigrationJobRepository : IRepository<ConfigurationMigrationJob>
{
    /// <summary>
    /// Gets a ConfigurationMigrationJob by its unique identifier.
    /// </summary>
    /// <param name="id">The job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ConfigurationMigrationJob or null if not found.</returns>
    Task<ConfigurationMigrationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new ConfigurationMigrationJob to the repository.
    /// </summary>
    /// <param name="job">The job to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(ConfigurationMigrationJob job, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing ConfigurationMigrationJob in the repository.
    /// </summary>
    /// <param name="job">The job to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(ConfigurationMigrationJob job, CancellationToken cancellationToken);
}