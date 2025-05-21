using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    /// <summary>
    /// Represents the result of a token validation operation.
    /// </summary>
    public record TokenValidationResult(bool IsValid, ClaimsPrincipal? ClaimsPrincipal, string? ErrorMessage, SecurityToken? ValidatedToken = null);

    /// <summary>
    /// Defines the contract for a service that validates authentication tokens.
    /// </summary>
    public interface ITokenValidationService
    {
        /// <summary>
        /// Validates the specified JWT token.
        /// </summary>
        /// <param name="token">The JWT token string.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="TokenValidationResult"/> indicating whether the token is valid and containing claims if successful.</returns>
        Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provides services for validating authentication tokens.
    /// Implements custom token validation logic, potentially fetching public keys from an
    /// Identity Provider's JWKS endpoint or performing custom claim validation as per REQ-3-010.
    /// </summary>
    public class TokenValidationService : ITokenValidationService
    {
        private readonly ILogger<TokenValidationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;
        private readonly string? _jwtAuthority;
        private readonly string? _jwtAudience;

        public TokenValidationService(IConfiguration configuration, ILogger<TokenValidationService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jwtAuthority = _configuration["SecurityConfigs:JwtAuthority"];
            _jwtAudience = _configuration["SecurityConfigs:JwtAudience"];

            if (string.IsNullOrWhiteSpace(_jwtAuthority))
            {
                _logger.LogWarning("JWT Authority (SecurityConfigs:JwtAuthority) is not configured. Standard JWT validation may be limited.");
            }
            else
            {
                // For fetching signing keys from IdP (JWKS endpoint)
                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{_jwtAuthority.TrimEnd('/')}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever { RequireHttps = _jwtAuthority.StartsWith("https://", StringComparison.OrdinalIgnoreCase) });
            }
        }

        /// <inheritdoc/>
        public async Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new TokenValidationResult(false, null, "Token is null or empty.");
            }

            if (_configurationManager == null || string.IsNullOrWhiteSpace(_jwtAuthority))
            {
                _logger.LogWarning("JWT Authority or ConfigurationManager not configured. Cannot perform full token validation via JWKS discovery.");
                // Fallback or error, depending on requirements.
                // This implementation will attempt standard validation if authority is missing, but it might fail without keys.
                // For a robust system, this should likely be a hard failure if authority is expected.
                return new TokenValidationResult(false, null, "Token validation service is not properly configured with an authority.");
            }

            try
            {
                var discoveryDocument = await _configurationManager.GetConfigurationAsync(cancellationToken);
                var signingKeys = discoveryDocument.SigningKeys;

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtAuthority, // Issuer from IdP

                    ValidateAudience = true,
                    ValidAudience = _jwtAudience, // Your API's audience

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys, // Keys from IdP

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1) // Allow for small clock differences
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                ClaimsPrincipal claimsPrincipal;

                claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // REQ-3-010: Custom claim validation can be added here if needed.
                // For example, check for specific roles or scopes:
                // if (!claimsPrincipal.IsInRole("RequiredRole"))
                // {
                //     _logger.LogWarning("Token validation failed: User does not have required role.");
                //     return new TokenValidationResult(false, null, "User does not have required role.");
                // }

                _logger.LogInformation("Token validated successfully for user: {User}", claimsPrincipal.Identity?.Name ?? "Unknown");
                return new TokenValidationResult(true, claimsPrincipal, null, validatedToken);
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Token expired.");
                return new TokenValidationResult(false, null, $"Token expired: {ex.Message}");
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Invalid signature.");
                return new TokenValidationResult(false, null, $"Invalid token signature: {ex.Message}");
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Invalid issuer.");
                return new TokenValidationResult(false, null, $"Invalid issuer: {ex.Message}");
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Invalid audience.");
                return new TokenValidationResult(false, null, $"Invalid audience: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during token validation.");
                return new TokenValidationResult(false, null, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}