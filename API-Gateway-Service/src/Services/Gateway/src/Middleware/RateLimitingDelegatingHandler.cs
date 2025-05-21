using GatewayService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IDistributedCacheService _cacheService;
        private readonly ILogger<RateLimitingDelegatingHandler> _logger;
        private readonly IConfiguration _configuration;

        public RateLimitingDelegatingHandler(
            IDistributedCacheService cacheService,
            ILogger<RateLimitingDelegatingHandler> logger,
            IConfiguration configuration)
        {
            _cacheService = cacheService;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // This is a basic example. Real-world rate limiting can be much more complex.
            // Ocelot has built-in rate limiting which might be preferred. This handler is for custom logic.

            // Try to get HttpContext to extract client IP. This is not always straightforward in DelegatingHandlers.
            // Ocelot might populate request.Properties["HttpContext"] or similar.
            HttpContext httpContext = null;
            if (request.Properties.TryGetValue("MS_HttpContext", out var context) && context is HttpContext)
            {
                httpContext = (HttpContext)context;
            } 
            else if (request.Properties.TryGetValue("HttpContext", out var contextOcelot) && contextOcelot is HttpContext) // Common in Ocelot
            {
                httpContext = (HttpContext)contextOcelot;
            }


            var clientIp = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown_ip";
            var path = request.RequestUri?.AbsolutePath ?? "unknown_path";
            
            // Example: Simple policy lookup (could be more dynamic)
            var policies = _configuration.GetSection("RateLimitingPolicies").Get<RateLimitPolicy[]>();
            var policy = policies?.FirstOrDefault(p => path.StartsWith(p.Endpoint) || p.Endpoint == "*");

            if (policy == null)
            {
                _logger.LogDebug("No specific rate limit policy found for path {Path}. Proceeding.", path);
                return await base.SendAsync(request, cancellationToken);
            }

            var cacheKey = $"RateLimit_{policy.RuleName ?? "Default"}_{clientIp}_{path}";
            var countString = await _cacheService.GetStringAsync(cacheKey, cancellationToken);
            int currentCount = 0;

            if (!string.IsNullOrEmpty(countString))
            {
                int.TryParse(countString, out currentCount);
            }

            if (currentCount >= policy.Limit)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientIp} on path {Path}. Policy: {PolicyName}, Limit: {Limit}, Period: {Period}", 
                    clientIp, path, policy.RuleName, policy.Limit, policy.Period);
                var response = new HttpResponseMessage((HttpStatusCode)429) // Too Many Requests
                {
                    ReasonPhrase = "Rate limit exceeded. Please try again later."
                };
                response.Headers.Add("Retry-After", policy.Period); // Inform client about the period
                return response;
            }

            currentCount++;
            var periodTimeSpan = GetTimeSpanFromPeriod(policy.Period);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = periodTimeSpan
            };

            await _cacheService.SetStringAsync(cacheKey, currentCount.ToString(), cacheOptions, cancellationToken);
            _logger.LogDebug("Rate limit check passed for client {ClientIp} on path {Path}. Count: {Count}", clientIp, path, currentCount);

            return await base.SendAsync(request, cancellationToken);
        }
        
        private static TimeSpan GetTimeSpanFromPeriod(string period)
        {
            if (string.IsNullOrEmpty(period) || period.Length < 2) return TimeSpan.FromSeconds(1); // Default
            
            var value = int.Parse(period.Substring(0, period.Length - 1));
            var unit = period.Substring(period.Length - 1).ToLower();

            return unit switch
            {
                "s" => TimeSpan.FromSeconds(value),
                "m" => TimeSpan.FromMinutes(value),
                "h" => TimeSpan.FromHours(value),
                "d" => TimeSpan.FromDays(value),
                _ => TimeSpan.FromSeconds(1),
            };
        }

        // Helper class to deserialize rate limiting policies from configuration
        private class RateLimitPolicy
        {
            public string Endpoint { get; set; }
            public string Period { get; set; }
            public int Limit { get; set; }
            public string RuleName { get; set; }
        }
    }
}