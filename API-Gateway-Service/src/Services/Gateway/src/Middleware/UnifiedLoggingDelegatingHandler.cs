using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Provides unified logging for requests and responses.
    /// Ocelot delegating handler or ASP.NET Core middleware that provides unified and structured logging
    /// for all requests and responses passing through the gateway. Captures essential information like
    /// request path, method, status code, latency, correlation ID, and user identity.
    /// </summary>
    public class UnifiedLoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<UnifiedLoggingDelegatingHandler> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public UnifiedLoggingDelegatingHandler(ILogger<UnifiedLoggingDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Attempt to get Correlation ID from request headers (should be set by CorrelationIdDelegatingHandler or client)
            string correlationId = "N/A";
            if (request.Headers.TryGetValues(CorrelationIdHeaderName, out var correlationValues))
            {
                correlationId = correlationValues.FirstOrDefault();
            }

            // Attempt to get HttpContext to extract User ID.
            // This depends on how Ocelot/ASP.NET Core pipeline is configured.
            string userId = "anonymous";
            HttpContext httpContext = null;
            if (request.Properties.TryGetValue("MS_HttpContext", out var contextMs) && contextMs is HttpContext msHttpContext)
            {
                httpContext = msHttpContext;
            }
            else if (request.Properties.TryGetValue("HttpContext", out var contextOcelot) && contextOcelot is HttpContext ocelotHttpContext) // Common in Ocelot
            {
                 httpContext = ocelotHttpContext;
            }
            
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                userId = httpContext.User.Identity.Name ?? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "authenticated_user_no_name";
            }


            _logger.LogInformation(
                "Request Starting: HTTP {RequestMethod} {RequestPath} | CorrelationId: {CorrelationId} | UserId: {UserId}",
                request.Method,
                request.RequestUri?.PathAndQuery,
                correlationId,
                userId);

            HttpResponseMessage response = null;
            Exception exception = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                _logger.LogError(ex, "Unhandled exception during request processing for {RequestPath} | CorrelationId: {CorrelationId}", request.RequestUri?.PathAndQuery, correlationId);
                throw; // Re-throw the exception to be handled by global error handling or Ocelot
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = response?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError; // Default if exception before response
                var logLevel = (int)statusCode >= 500 ? LogLevel.Error : LogLevel.Information;

                _logger.Log(logLevel,
                    "Request Finished: HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {ElapsedMilliseconds}ms | CorrelationId: {CorrelationId} | UserId: {UserId}",
                    request.Method,
                    request.RequestUri?.PathAndQuery,
                    (int)statusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId,
                    userId);
            }
            
            return response;
        }
    }
}