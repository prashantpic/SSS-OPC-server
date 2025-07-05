using Opc.System.Services.Authentication.Domain.Entities;

namespace Opc.System.Services.Authentication.Application.Interfaces;

/// <summary>
/// A contract for the user repository, defining all required interactions with the user data store.
/// This abstracts the data access logic for user entities from the application layer.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ApplicationUser if found; otherwise, null.</returns>
    Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ApplicationUser if found; otherwise, null.</returns>
    Task<ApplicationUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Anonymizes a user's Personally Identifiable Information (PII) to comply with DSAR 'right to be forgotten' requests.
    /// </summary>
    /// <param name="id">The unique identifier of the user to anonymize.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the user was found and anonymized; otherwise, false.</returns>
    Task<bool> AnonymizeUserAsync(Guid id, CancellationToken cancellationToken = default);
}