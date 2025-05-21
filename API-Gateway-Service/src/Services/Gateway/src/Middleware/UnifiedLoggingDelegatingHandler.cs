using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
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
            var httpContext = request.Properties["HttpContext"] as HttpContext;
            string? correlationId = null;

            if (httpContext != null)
            {
                if (httpContext.Items.TryGetValue(CorrelationIdHeaderName, out var cidObj) && cidObj is string cidStr)
                {
                    correlationId = cidStr;
                }
                else if (httpContext.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var cidValues))
                {
                    correlationId = cidValues.FirstOrDefault();
                }
            }
            
            // If still null, check request headers directly (though CorrelationIdDelegatingHandler should have set it)
            if (string.IsNullOrEmpty(correlationId) && request.Headers.TryGetValues(CorrelationIdHeaderName, out var headerValues))
            {
                correlationId = headerValues.FirstOrDefault();
            }
            
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString(); // Fallback if not set by previous handler
                _logger.LogWarning("Correlation ID not found, generated new one: {CorrelationId}", correlationId);
            }

            var downstreamRoute = httpContext?.Items.DownstreamRoute();
            var upstreamPath = downstreamRoute?.UpstreamPathTemplate.OriginalValue ?? request.RequestUri?.AbsolutePath ?? "UnknownPath";
            var downstreamPath = downstreamRoute?.DownstreamPathTemplate.OriginalValue ?? request.RequestUri?.ToString() ?? "UnknownDownstream";
            var clientIp = httpContext?.Connection.RemoteIpAddress?.ToString();
            var userId = httpContext?.User?.Identity?.IsAuthenticated == true 
                ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? httpContext.User.Identity.Name
                : "Anonymous";

            _logger.LogInformation(
                "Gateway Request START: CorrelationId={CorrelationId}, User={UserId}, ClientIp={ClientIp}, Method={Method}, UpstreamPath={UpstreamPath}, DownstreamPath={DownstreamPath}",
                correlationId, userId, clientIp, request.Method, upstreamPath, downstreamPath);
            
            // Optionally log request headers (be careful with sensitive data)
            // _logger.LogDebug("Request Headers: {Headers}", request.Headers);

            var stopwatch = Stopwatch.StartNew();
            HttpResponseMessage? response = null;
            Exception? exception = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                exception = ex;
                _logger.LogError(ex, "Exception during downstream request for CorrelationId={CorrelationId}", correlationId);
                throw; // Re-throw the exception to be handled by Ocelot/ASP.NET Core
            }
            finally
            {
                stopwatch.Stop();
                var statusCode = response?.StatusCode;
                var reasonPhrase = response?.ReasonPhrase;

                if (exception != null) // If an exception occurred and was re-thrown
                {
                    _logger.LogError(
                        "Gateway Request FAILED: CorrelationId={CorrelationId}, User={UserId}, ClientIp={ClientIp}, Method={Method}, UpstreamPath={UpstreamPath}, DownstreamPath={DownstreamPath}, DurationMs={DurationMs}, Error={ErrorMessage}",
                        correlationId, userId, clientIp, request.Method, upstreamPath, downstreamPath, stopwatch.ElapsedMilliseconds, exception.Message);
                }
                else // Normal completion or handled error resulting in a response
                {
                     if (response != null && response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            "Gateway Request END: CorrelationId={CorrelationId}, User={UserId}, ClientIp={ClientIp}, Method={Method}, UpstreamPath={UpstreamPath}, DownstreamPath={DownstreamPath}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, DurationMs={DurationMs}",
                            correlationId, userId, clientIp, request.Method, upstreamPath, downstreamPath, (int?)statusCode, reasonPhrase, stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Gateway Request END (non-success): CorrelationId={CorrelationId}, User={UserId}, ClientIp={ClientIp}, Method={Method}, UpstreamPath={UpstreamPath}, DownstreamPath={DownstreamPath}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, DurationMs={DurationMs}",
                            correlationId, userId, clientIp, request.Method, upstreamPath, downstreamPath, (int?)statusCode, reasonPhrase, stopwatch.ElapsedMilliseconds);
                    }
                }
                // Optionally log response headers (be careful with sensitive data)
                // if (response != null) _logger.LogDebug("Response Headers: {Headers}", response.Headers);
            }
        }
    }
}