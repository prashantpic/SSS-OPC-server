using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed; // For DistributedCacheEntryOptions

namespace GatewayService.Middleware
{
    /// <summary>
    /// Provides advanced rate limiting capabilities.
    /// Implements custom rate limiting logic for API Gateway requests, potentially using a distributed cache 
    /// or more complex algorithms than Ocelot's built-in options. 
    /// Can apply policies based on client ID, IP address, or other request characteristics.
    /// </summary>
    public class RateLimitingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<RateLimitingDelegatingHandler> _logger;
        private readonly IDistributedCacheService _cacheService;
        // In a real scenario, rate limit policies would be configurable
        // For simplicity, using fixed values here or they could be read from Ocelot route config metadata
        private const int DefaultLimit = 100;
        private readonly TimeSpan DefaultPeriod = TimeSpan.FromMinutes(1);

        public RateLimitingDelegatingHandler(
            ILogger<RateLimitingDelegatingHandler> logger,
            IDistributedCacheService cacheService)
        {
            _logger = logger;
            _cacheService = cacheService;
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

            // Ocelot's built-in rate limiting might be enabled. This handler is for *custom* logic
            // if Ocelot's options (EnableRateLimiting, ClientWhitelist, Period, PeriodTimespan, Limit in ocelot.json)
            // are insufficient or need more dynamic handling.
            // We'll assume this handler is applied only if Ocelot's built-in one is not used for this route,
            // or if we want to augment it.

            // Example: Use a custom rate limiting policy defined in route metadata or a global policy
            // This example uses Client IP. A more robust solution might use API Key or User ID.
            var clientIdentifier = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
            if (httpContext.User?.Identity?.IsAuthenticated == true)
            {
                clientIdentifier = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? clientIdentifier;
            }

            // You could fetch policy details (limit, period) from DownstreamRoute.RateLimitOptions
            // or custom metadata if Ocelot's built-in mechanism isn't fully utilized.
            var rateLimitOptions = downstreamRoute.RateLimitOptions;
            var limit = rateLimitOptions.EnableRateLimiting ? rateLimitOptions.Limit : DefaultLimit;
            var period = rateLimitOptions.EnableRateLimiting && rateLimitOptions.PeriodTimespan > 0 
                ? TimeSpan.FromSeconds(rateLimitOptions.PeriodTimespan) 
                : DefaultPeriod;

            var cacheKey = $"RateLimit_{downstreamRoute.UpstreamPathTemplate.OriginalValue}_{clientIdentifier}";

            var count = await _cacheService.GetJsonAsync<long?>(cacheKey);

            if (count.HasValue && count.Value >= limit)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientIdentifier} on route {Route}. Limit: {Limit}, Period: {Period}",
                    clientIdentifier, downstreamRoute.UpstreamPathTemplate.OriginalValue, limit, period);
                var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                response.Headers.Add("Retry-After", period.TotalSeconds.ToString()); // Or calculate remaining time
                return response;
            }

            var newCount = (count ?? 0) + 1;
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = period
            };
            await _cacheService.SetJsonAsync(cacheKey, newCount, options, cancellationToken);
            
            _logger.LogDebug("Request count for client {ClientIdentifier} on route {Route} is {Count}/{Limit}",
                clientIdentifier, downstreamRoute.UpstreamPathTemplate.OriginalValue, newCount, limit);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}