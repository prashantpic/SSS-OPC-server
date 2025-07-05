using Opc.System.Services.Authentication.Domain.Entities;

namespace Opc.System.Services.Authentication.Application.Interfaces;

/// <summary>
/// A contract for the JWT generation service, which is a core part of the 'TokenGenerationService' component.
/// This abstracts the token generation logic, allowing for different implementations or configurations.
/// </summary>
public interface IJwtGenerator
{
    /// <summary>
    /// Creates a signed JSON Web Token (JWT) for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token is being generated.</param>
    /// <param name="roles">A list of roles assigned to the user, to be included in the token claims.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}