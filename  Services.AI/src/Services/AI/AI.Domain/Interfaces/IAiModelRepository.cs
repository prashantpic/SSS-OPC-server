using Opc.System.Services.AI.Domain.Aggregates;

namespace Opc.System.Services.AI.Domain.Interfaces;

/// <summary>
/// Interface defining the contract for data persistence operations for the AiModel aggregate.
/// It abstracts the data storage mechanism, allowing the domain to remain persistence-ignorant.
/// </summary>
public interface IAiModelRepository
{
    /// <summary>
    /// Finds an AiModel aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The model's unique identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The AiModel aggregate instance or null if not found.</returns>
    Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new AiModel aggregate to the persistence store.
    /// </summary>
    /// <param name="model">The model to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(AiModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing AiModel aggregate in the persistence store.
    /// </summary>
    /// <param name="model">The model to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(AiModel model, CancellationToken cancellationToken = default);
}