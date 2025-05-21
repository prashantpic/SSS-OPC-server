using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Application.Abstractions.Clients;

// Repository interface for managing ClientConfiguration aggregate data.
public interface IClientConfigurationRepository : IRepository<ClientConfiguration>
{
    /// <summary>
    /// Gets a ClientConfiguration by its unique identifier, including its versions.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ClientConfiguration or null if not found.</returns>
    Task<ClientConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the ClientConfiguration associated with a specific ClientInstance, including versions.
    /// </summary>
    /// <param name="clientInstanceId">The ID of the client instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ClientConfiguration or null if not found.</returns>
    Task<ClientConfiguration?> GetByClientInstanceIdAsync(Guid clientInstanceId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new ClientConfiguration to the repository.
    /// </summary>
    /// <param name="clientConfiguration">The configuration to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(ClientConfiguration clientConfiguration, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing ClientConfiguration in the repository.
    /// </summary>
    /// <param name="clientConfiguration">The configuration to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(ClientConfiguration clientConfiguration, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a ClientConfiguration by its unique identifier.
    /// </summary>
    /// <param name="id">The configuration ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a specific ConfigurationVersion by its ID.
    /// </summary>
    /// <param name="versionId">The version ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ConfigurationVersion or null if not found.</returns>
    Task<ConfigurationVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a ClientConfiguration by its ID, ensuring a specific version is loaded.
    /// </summary>
    /// <param name="configId">The configuration ID.</param>
    /// <param name="versionId">The specific version ID to ensure is loaded.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ClientConfiguration with the specified version (if exists), or null.</returns>
    Task<ClientConfiguration?> GetByIdWithVersionAsync(Guid configId, Guid versionId, CancellationToken cancellationToken);
}