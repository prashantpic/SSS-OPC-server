using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Handles custom JWT validation logic for Ocelot routes.
    /// This can include custom claim checks, interacting with an external validation service, 
    /// or logging specific details related to token validation as per REQ-3-010.
    /// </summary>
    public class JwtValidationDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<JwtValidationDelegatingHandler> _logger;
        private readonly ITokenValidationService _tokenValidationService;

        public JwtValidationDelegatingHandler(
            ILogger<JwtValidationDelegatingHandler> logger,
            ITokenValidationService tokenValidationService)
        {
            _logger = logger;
            _tokenValidationService = tokenValidationService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = request.Properties["HttpContext"] as HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is not available in HttpRequestMessage properties.");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Error processing request.") };
            }

            var downstreamRoute = httpContext.Items.DownstreamRoute();
            
            // Check if authentication is required for the route
            if (downstreamRoute.IsAuthenticated)
            {
                string? token = null;
                if (request.Headers.Authorization != null && request.Headers.Authorization.Scheme.Equals("Bearer", System.StringComparison.OrdinalIgnoreCase))
                {
                    token = request.Headers.Authorization.Parameter;
                }

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Route {Route} requires authentication, but no token was provided.", downstreamRoute.UpstreamPathTemplate.OriginalValue);
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("Authorization token is missing.") };
                }

                var (isValid, principal, errorMessage) = await _tokenValidationService.ValidateTokenAsync(token, "Bearer");

                if (!isValid)
                {
                    _logger.LogWarning("JWT validation failed for route {Route}. Error: {ErrorMessage}", downstreamRoute.UpstreamPathTemplate.OriginalValue, errorMessage);
                    // REQ-3-010: If invalid, return 401 or 403.
                    // Depending on the error, could be 403 if token is valid but lacks permissions.
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent(errorMessage ?? "Invalid token.") };
                }

                if (principal != null)
                {
                    // Potentially add validated claims to downstream headers or update HttpContext.User
                    // Ocelot can map claims to downstream headers using AddClaimsToRequest configuration.
                    // For direct manipulation or complex logic:
                    // foreach (var claim in principal.Claims)
                    // {
                    //    request.Headers.TryAddWithoutValidation($"X-Claim-{claim.Type}", claim.Value);
                    // }
                    httpContext.User = principal; // Make the validated principal available
                    _logger.LogInformation("JWT successfully validated for user {User} on route {Route}.", principal.Identity?.Name ?? "unknown", downstreamRoute.UpstreamPathTemplate.OriginalValue);
                }
                else
                {
                     _logger.LogWarning("Token was considered valid by ITokenValidationService, but no ClaimsPrincipal was returned.");
                }

                // Check scopes if defined in Ocelot route configuration
                if (downstreamRoute.AuthenticationOptions.AllowedScopes != null && downstreamRoute.AuthenticationOptions.AllowedScopes.Any())
                {
                    var scopeClaim = principal?.Claims.FirstOrDefault(c => c.Type == "scope" || c.Type == "scp");
                    if (scopeClaim == null || !downstreamRoute.AuthenticationOptions.AllowedScopes.Any(s => scopeClaim.Value.Split(' ').Contains(s)))
                    {
                        _logger.LogWarning("User does not have the required scopes for route {Route}. Required: {Scopes}", 
                            downstreamRoute.UpstreamPathTemplate.OriginalValue, 
                            string.Join(",", downstreamRoute.AuthenticationOptions.AllowedScopes));
                        return new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("Insufficient scope.") };
                    }
                }
            }
            else
            {
                _logger.LogDebug("Route {Route} does not require authentication.", downstreamRoute.UpstreamPathTemplate.OriginalValue);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}