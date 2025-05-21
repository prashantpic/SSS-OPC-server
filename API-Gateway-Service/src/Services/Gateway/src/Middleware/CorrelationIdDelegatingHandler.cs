using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
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
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdDelegatingHandler(ILogger<CorrelationIdDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string correlationId = GetOrGenerateCorrelationId(request);

            // Add Correlation ID to the outgoing request headers for downstream services
            if (!request.Headers.Contains(CorrelationIdHeaderName))
            {
                request.Headers.Add(CorrelationIdHeaderName, correlationId);
                _logger.LogDebug("Added Correlation ID {CorrelationId} to outgoing request headers for {RequestUri}", correlationId, request.RequestUri);
            }
            else
            {
                 // If it already exists (e.g. set by client), ensure it's a single valid value or overwrite if necessary
                var existingIds = request.Headers.GetValues(CorrelationIdHeaderName).ToList();
                if (existingIds.Count > 1 || string.IsNullOrWhiteSpace(existingIds.First()))
                {
                    request.Headers.Remove(CorrelationIdHeaderName);
                    request.Headers.Add(CorrelationIdHeaderName, correlationId); // Use the one we decided on
                    _logger.LogDebug("Replaced/Ensured single Correlation ID {CorrelationId} in outgoing request headers for {RequestUri}", correlationId, request.RequestUri);
                } else {
                    correlationId = existingIds.First(); // Use the existing valid one
                }
            }


            // To make it available for other handlers or logging within the gateway for this request scope:
            // If this handler is used as ASP.NET Core middleware, HttpContext.Items can be used.
            // For Ocelot DelegatingHandler, Ocelot sets up `DownstreamContext.HttpContext`.
            // We can try to access HttpContext via request.Properties.
            if (request.Properties.TryGetValue("MS_HttpContext", out var context) && context is HttpContext httpContext)
            _logger.LogDebug("MS_HttpContext found for correlation ID handling.");
            {
                httpContext.Items[CorrelationIdHeaderName] = correlationId;
            }
            else if (request.Properties.TryGetValue("HttpContext", out var ocelotContext) && ocelotContext is HttpContext ocelotHttpContext) // Common in Ocelot
            {
                 _logger.LogDebug("Ocelot HttpContext found for correlation ID handling.");
                ocelotHttpContext.Items[CorrelationIdHeaderName] = correlationId;
            }
            else
            {
                _logger.LogWarning("Could not retrieve HttpContext from request.Properties to store Correlation ID in HttpContext.Items. It will still be propagated in headers.");
            }
            
            // Also ensure the response carries the correlation ID
            var response = await base.SendAsync(request, cancellationToken);
            
            if (!response.Headers.Contains(CorrelationIdHeaderName))
            {
                response.Headers.Add(CorrelationIdHeaderName, correlationId);
            }

            return response;
        }

        private string GetOrGenerateCorrelationId(HttpRequestMessage request)
        {
            if (request.Headers.TryGetValues(CorrelationIdHeaderName, out var values))
            {
                string id = values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                if (!string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogDebug("Using existing Correlation ID {CorrelationId} from request headers for {RequestUri}", id, request.RequestUri);
                    return id;
                }
            }
            
            // Try to get from HttpContext if it was set by an earlier middleware (e.g. if this is chained)
            // This is less likely if this handler is the primary source of CorrelationID for Ocelot
             if (request.Properties.TryGetValue("MS_HttpContext", out var context) && context is HttpContext httpContext)
            {
                 if(httpContext.Items.TryGetValue(CorrelationIdHeaderName, out var  contextItemId) && contextItemId is string idFromContext && !string.IsNullOrWhiteSpace(idFromContext))
                 {
                    _logger.LogDebug("Using existing Correlation ID {CorrelationId} from HttpContext.Items for {RequestUri}", idFromContext, request.RequestUri);
                    return idFromContext;
                 }
            }
             else if (request.Properties.TryGetValue("HttpContext", out var ocelotContext) && ocelotContext is HttpContext ocelotHttpContext) // Common in Ocelot
            {
                if(ocelotHttpContext.Items.TryGetValue(CorrelationIdHeaderName, out var  contextItemId) && contextItemId is string idFromContext && !string.IsNullOrWhiteSpace(idFromContext))
                 {
                    _logger.LogDebug("Using existing Correlation ID {CorrelationId} from Ocelot HttpContext.Items for {RequestUri}", idFromContext, request.RequestUri);
                    return idFromContext;
                 }
            }


            var newCorrelationId = Guid.NewGuid().ToString();
            _logger.LogDebug("Generated new Correlation ID {CorrelationId} for {RequestUri}", newCorrelationId, request.RequestUri);
            return newCorrelationId;
        }
    }
}