using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace GatewayService.Services
{
    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public ClaimsPrincipal? Principal { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
    }

    public class TokenValidationService : ITokenValidationService
    {
        private readonly ILogger<TokenValidationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _authority;
        private readonly string? _audience;
        private IConfigurationManager<OpenIdConnectConfiguration>? _configurationManager;


        public TokenValidationService(
            ILogger<TokenValidationService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            _authority = _configuration["SecurityConfigs:JwtAuthority"];
            _audience = _configuration["SecurityConfigs:JwtAudience"];

            if (string.IsNullOrEmpty(_authority))
            {
                _logger.LogWarning("JWT Authority is not configured in SecurityConfigs:JwtAuthority. Token validation might be limited.");
            }
            else
            {
                // Initialize configuration manager for fetching OIDC discovery document and JWKS
                _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{_authority.TrimEnd('/')}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever(_httpClientFactory.CreateClient("OidcClient")) { RequireHttps = _authority.StartsWith("https", StringComparison.OrdinalIgnoreCase) }
                );
            }
        }

        public async Task<TokenValidationResult> ValidateTokenAsync(string token, string scheme = "Bearer")
        {
            if (string.IsNullOrEmpty(token))
            {
                return new TokenValidationResult { IsValid = false, ErrorMessage = "Token is null or empty." };
            }

            if (string.IsNullOrEmpty(_authority) || _configurationManager == null)
            {
                _logger.LogWarning("JWT Authority or ConfigurationManager is not configured. Skipping full validation.");
                // Potentially perform very basic JWT parsing if needed, but full validation is not possible.
                // For now, consider it invalid if authority is missing for robust security.
                return new TokenValidationResult { IsValid = false, ErrorMessage = "JWT Authority not configured for validation." };
            }

            try
            {
                var discoveryDocument = await _configurationManager.GetConfigurationAsync(default);
                var signingKeys = discoveryDocument.SigningKeys;

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _authority,

                    ValidateAudience = !string.IsNullOrEmpty(_audience), // Validate audience only if configured
                    ValidAudience = _audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = signingKeys, // Use keys from JWKS endpoint

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5) // Allow for some clock drift
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

                if (validatedToken == null)
                {
                    _logger.LogWarning("Token validation failed: validatedToken is null.");
                    return new TokenValidationResult { IsValid = false, ErrorMessage = "Token validation resulted in null security token." };
                }

                // REQ-3-010: Placeholder for custom claim validation or token introspection
                // Example: Check for a specific scope or claim
                // if (!principal.HasClaim(c => c.Type == "scope" && c.Value.Split(' ').Contains("api.read")))
                // {
                //     _logger.LogWarning("Token validation failed: Missing required scope 'api.read'.");
                //     return new TokenValidationResult { IsValid = false, Principal = principal, ErrorMessage = "Missing required scope." };
                // }

                // Example: Placeholder for token introspection call to Security Service
                // var introspectionEndpoint = _configuration["SecurityConfigs:IntrospectionEndpoint"];
                // if (!string.IsNullOrEmpty(introspectionEndpoint))
                // {
                //     var introspectionClient = _httpClientFactory.CreateClient("IntrospectionClient");
                //     var introspectionResponse = await introspectionClient.PostAsync(introspectionEndpoint, new FormUrlEncodedContent(new Dictionary<string, string> { { "token", token } }));
                //     if (!introspectionResponse.IsSuccessStatusCode)
                //     {
                //         _logger.LogWarning("Token introspection failed with status: {StatusCode}", introspectionResponse.StatusCode);
                //         return new TokenValidationResult { IsValid = false, Principal = principal, ErrorMessage = "Token introspection failed." };
                //     }
                //     var introspectionResult = await introspectionResponse.Content.ReadFromJsonAsync<IntrospectionResult>(); // Define IntrospectionResult class
                //     if (introspectionResult == null || !introspectionResult.Active)
                //     {
                //         _logger.LogWarning("Token introspection reported token as inactive.");
                //         return new TokenValidationResult { IsValid = false, Principal = principal, ErrorMessage = "Token is not active (via introspection)." };
                //     }
                // }


                _logger.LogInformation("Token validated successfully for issuer: {Issuer}", validatedToken.Issuer);
                return new TokenValidationResult { IsValid = true, Principal = principal };
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Token is expired.");
                return new TokenValidationResult { IsValid = false, ErrorMessage = "Token has expired.", Exception = ex };
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "Token validation failed: Invalid signature.");
                return new TokenValidationResult { IsValid = false, ErrorMessage = "Token signature is invalid.", Exception = ex };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during token validation.");
                return new TokenValidationResult { IsValid = false, ErrorMessage = $"An unexpected error occurred: {ex.Message}", Exception = ex };
            }
        }

        public async Task<bool> IsTokenValidAsync(string token, string scheme = "Bearer")
        {
            var result = await ValidateTokenAsync(token, scheme);
            return result.IsValid;
        }

        // Placeholder for a potential IntrospectionResult DTO
        // private class IntrospectionResult
        // {
        //     public bool Active { get; set; }
        //     // Add other fields as needed, e.g., scope, client_id, username, exp
        // }
    }
}