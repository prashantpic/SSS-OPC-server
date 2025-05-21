using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Application.Abstractions.Clients;

// Repository interface for managing ClientInstance aggregate data.
public interface IClientInstanceRepository : IRepository<ClientInstance>
{
    /// <summary>
    /// Gets a ClientInstance by its unique identifier.
    /// </summary>
    /// <param name="id">The client instance ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ClientInstance or null if not found.</returns>
    Task<ClientInstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new ClientInstance to the repository.
    /// </summary>
    /// <param name="clientInstance">The client instance to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(ClientInstance clientInstance, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing ClientInstance in the repository.
    /// </summary>
    /// <param name="clientInstance">The client instance to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(ClientInstance clientInstance, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all ClientInstances.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A read-only list of all client instances.</returns>
    Task<IReadOnlyList<ClientInstance>> ListAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a ClientInstance by its name.
    /// </summary>
    /// <param name="name">The name of the client instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ClientInstance or null if not found.</returns>
    Task<ClientInstance?> GetByNameAsync(string name, CancellationToken cancellationToken);
}