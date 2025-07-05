using ManagementService.Domain.Aggregates;

namespace ManagementService.Application.Contracts.Persistence;

/// <summary>
/// Defines the contract for persistence operations for the OpcClientInstance aggregate.
/// This abstracts the data storage mechanism from the application logic.
/// </summary>
public interface IOpcClientInstanceRepository
{
    /// <summary>
    /// Retrieves a client instance by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the client instance.</param>
    /// <returns>The found OpcClientInstance or null if not found.</returns>
    Task<OpcClientInstance?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves a read-only list of all managed client instances.
    /// </summary>
    /// <returns>A list of all client instances.</returns>
    Task<IReadOnlyList<OpcClientInstance>> GetAllAsync();

    /// <summary>
    /// Adds a new client instance to the persistence store.
    /// </summary>
    /// <param name="clientInstance">The new client instance to add.</param>
    Task AddAsync(OpcClientInstance clientInstance);

    /// <summary>
    /// Updates an existing client instance in the persistence store.
    /// </summary>
    /// <param name="clientInstance">The client instance with updated state.</param>
    Task UpdateAsync(OpcClientInstance clientInstance);
}