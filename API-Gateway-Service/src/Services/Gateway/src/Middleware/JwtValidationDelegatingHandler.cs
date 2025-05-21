using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Handles custom JWT validation logic for Ocelot routes.
    /// This handler complements or replaces Ocelot's built-in JWT logic if more control is needed (REQ-3-010).
    /// </summary>
    public class JwtValidationDelegatingHandler : DelegatingHandler
    {
        private readonly ITokenValidationService _tokenValidationService;
        private readonly ILogger<JwtValidationDelegatingHandler> _logger;

        public JwtValidationDelegatingHandler(
            ITokenValidationService tokenValidationService,
            ILogger<JwtValidationDelegatingHandler> logger)
        {
            _tokenValidationService = tokenValidationService;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("JwtValidationDelegatingHandler: Intercepting request for custom JWT validation.");

            AuthenticationHeaderValue authHeader = request.Headers.Authorization;
            if (authHeader == null || !string.Equals(authHeader.Scheme, "Bearer", System.StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("JwtValidationDelegatingHandler: No Bearer token found in Authorization header.");
                // Let Ocelot's built-in authentication handle it or proceed if auth is not required for the route.
                // If this handler *must* validate, return 401 here.
                // For this example, we assume Ocelot's auth middleware runs first or this is an augmentation.
                return await base.SendAsync(request, cancellationToken);
            }

            string token = authHeader.Parameter;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("JwtValidationDelegatingHandler: Bearer token is empty.");
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Bearer token is missing." };
            }

            var validationResult = await _tokenValidationService.ValidateTokenAsync(token, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("JwtValidationDelegatingHandler: Token validation failed. Reason: {Reason}", validationResult.Error);
                // REQ-3-010: Enforce authorization policies
                // Depending on policy, could be 401 Unauthorized or 403 Forbidden
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = validationResult.Error ?? "Invalid token" };
            }

            _logger.LogInformation("JwtValidationDelegatingHandler: Token validated successfully for user: {User}", validationResult.ClaimsPrincipal?.Identity?.Name ?? "Unknown");

            // Optionally, attach the validated ClaimsPrincipal to the HttpContext if downstream Ocelot handlers need it
            // This is tricky with DelegatingHandler as HttpContext isn't directly passed like in ASP.NET Core middleware.
            // Ocelot's `DownstreamContext.HttpContext.User` would typically be set by Ocelot's own auth middleware.
            // If this handler *replaces* Ocelot's JWT validation, it would need to populate `request.Properties`
            // for Ocelot to pick up, or rely on Ocelot's AuthenticationMiddleware to run after this.

            // For REQ-3-010, if further claim-based authorization specific to Ocelot routes is needed
            // beyond what the standard Ocelot `RouteClaimsRequirement` offers, it could be done here.
            // However, Ocelot's `RouteClaimsRequirement` is typically sufficient.

            return await base.SendAsync(request, cancellationToken);
        }
    }
}