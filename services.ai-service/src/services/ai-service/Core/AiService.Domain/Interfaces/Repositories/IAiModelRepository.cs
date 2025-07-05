using AiService.Domain.Aggregates.AiModel;

namespace AiService.Domain.Interfaces.Repositories;

/// <summary>
/// Contract for storing and retrieving AiModel aggregates.
/// This interface abstracts the persistence mechanism for AI models from the domain logic.
/// </summary>
public interface IAiModelRepository
{
    /// <summary>
    /// Retrieves an AI model by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the model.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AiModel"/> or null if not found.</returns>
    Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new AI model to the repository.
    /// </summary>
    /// <param name="model">The <see cref="AiModel"/> to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(AiModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing AI model in the repository.
    /// </summary>
    /// <param name="model">The <see cref="AiModel"/> to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(AiModel model, CancellationToken cancellationToken = default);
}