using System.Security.Claims;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    /// <summary>
    /// Interface for token validation operations.
    /// Defines the contract for a service responsible for advanced or custom token validation logic.
    /// REQ-3-010
    /// </summary>
    public interface ITokenValidationService
    {
        /// <summary>
        /// Validates the given token.
        /// </summary>
        /// <param name="token">The token string to validate.</param>
        /// <param name="scheme">The authentication scheme (e.g., "Bearer").</param>
        /// <returns>A tuple indicating if the token is valid, the ClaimsPrincipal if valid, and an error message if invalid.</returns>
        Task<(bool IsValid, ClaimsPrincipal? Principal, string? ErrorMessage)> ValidateTokenAsync(string token, string scheme);
    }
}