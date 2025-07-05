using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

/// <summary>
/// Defines a contract for creating security tokens.
/// This interface abstracts the creation of JWT access tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated JWT token string.</returns>
    Task<string> GenerateAccessToken(ApplicationUser user);
}