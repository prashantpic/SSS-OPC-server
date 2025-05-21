using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware; // For DownstreamRoute
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Manages correlation IDs for distributed tracing.
    /// Ocelot delegating handler or ASP.NET Core middleware responsible for managing correlation IDs. 
    /// It ensures that a correlation ID is present in requests, generating one if necessary, 
    /// and propagates it to downstream services to enable distributed tracing.
    /// </summary>
    public class CorrelationIdDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<CorrelationIdDelegatingHandler> _logger;
        public const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdDelegatingHandler(ILogger<CorrelationIdDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string correlationId;

            // Try to get correlation ID from HttpContext.Items (if set by an earlier ASP.NET Core middleware)
            var httpContext = request.Properties.TryGetValue("HttpContext", out var ctx) && ctx is HttpContext hCtx ? hCtx : null;

            if (httpContext != null && httpContext.Items.TryGetValue(CorrelationIdHeaderName, out var correlationIdFromContext) && correlationIdFromContext is string ctxCorrelationId)
            {
                correlationId = ctxCorrelationId;
                _logger.LogDebug("Using Correlation ID from HttpContext.Items: {CorrelationId}", correlationId);
            }
            // Try to get correlation ID from incoming request headers
            else if (request.Headers.TryGetValues(CorrelationIdHeaderName, out var correlationValues) && correlationValues.FirstOrDefault() is string headerCorrelationId && !string.IsNullOrWhiteSpace(headerCorrelationId))
            {
                correlationId = headerCorrelationId;
                _logger.LogDebug("Using Correlation ID from request header: {CorrelationId}", correlationId);
            }
            else
            {
                // Generate a new correlation ID if not present
                correlationId = Guid.NewGuid().ToString();
                _logger.LogDebug("Generated new Correlation ID: {CorrelationId}", correlationId);
            }

            // Add/overwrite the correlation ID to the outgoing request headers for downstream services
            if (request.Headers.Contains(CorrelationIdHeaderName))
            {
                request.Headers.Remove(CorrelationIdHeaderName);
            }
            request.Headers.TryAddWithoutValidation(CorrelationIdHeaderName, correlationId);

            // Store in HttpContext.Items for other handlers/middleware in the gateway's pipeline
            if (httpContext != null)
            {
                httpContext.Items[CorrelationIdHeaderName] = correlationId;

                // Also add to response headers for the client
                httpContext.Response.OnStarting(() =>
                {
                    if (!httpContext.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                    {
                        httpContext.Response.Headers.Add(CorrelationIdHeaderName, correlationId);
                    }
                    return Task.CompletedTask;
                });
            }
            
            _logger.LogInformation("Propagating Correlation ID {CorrelationId} to downstream service for request {RequestUri}", correlationId, request.RequestUri);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}