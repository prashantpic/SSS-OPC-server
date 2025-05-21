using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    /// <summary>
    /// Represents the result of a token validation attempt.
    /// </summary>
    public record TokenValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the token validation was successful.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Gets the ClaimsPrincipal generated from the validated token.
        /// Null if validation failed.
        /// </summary>
        public ClaimsPrincipal ClaimsPrincipal { get; init; }

        /// <summary>
        /// Gets an error message if validation failed.
        /// Null if validation was successful.
        /// </summary>
        public string Error { get; init; }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static TokenValidationResult Success(ClaimsPrincipal principal) =>
            new() { IsValid = true, ClaimsPrincipal = principal };

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        public static TokenValidationResultFailed Failed(string error) =>
            new() { IsValid = false, Error = error };
    }

    // Alias for failed result to improve readability
    public record TokenValidationResultFailed : TokenValidationResult;


    /// <summary>
    /// Interface for token validation operations.
    /// Defines the contract for a service responsible for advanced or custom token validation logic (REQ-3-010).
    /// </summary>
    public interface ITokenValidationService
    {
        /// <summary>
        /// Validates the specified JWT token.
        /// </summary>
        /// <param name="token">The JWT token string.</param>
        /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
        /// <returns>A Task representing the asynchronous operation, with a TokenValidationResult.</returns>
        Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    }
}