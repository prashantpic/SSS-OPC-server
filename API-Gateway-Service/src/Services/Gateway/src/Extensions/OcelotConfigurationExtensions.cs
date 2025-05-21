using GatewayService.Aggregation;
using GatewayService.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Provider.Polly; // Required for Polly QoS with Ocelot
using Ocelot.Cache.CacheManager; // If using CacheManager, or Ocelot.Cache.Distributed for IDistributedCache

namespace GatewayService.Extensions
{
    public static class OcelotConfigurationExtensions
    {
        public static IServiceCollection AddOcelotServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure Ocelot's caching to use the globally registered IDistributedCache (e.g., Redis)
            // Ensure Ocelot.Cache.Distributed package is referenced if using this.
            // Or use Ocelot.Cache.CacheManager for CacheManager integration.
            services.AddOcelot(configuration)
                .AddPolly() // Adds Polly QoS provider
                .AddCacheManager(x => // Example using CacheManager, replace if using Ocelot.Cache.Distributed directly
                {
                    x.WithDictionaryHandle(); // Example, configure actual cache provider here or rely on Ocelot finding IDistributedCache
                })
                // Register custom delegating handlers. The boolean parameter indicates global registration.
                // If false, they must be specified per-route in ocelot.json.
                .AddDelegatingHandler<JwtValidationDelegatingHandler>(false) // Typically route-specific or handled by Ocelot's Auth
                .AddDelegatingHandler<RateLimitingDelegatingHandler>(false) // Can be global or route-specific
                .AddDelegatingHandler<RequestTransformationDelegatingHandler>(false) // Usually route-specific
                .AddDelegatingHandler<ResponseTransformationDelegatingHandler>(false) // Usually route-specific
                .AddDelegatingHandler<CorrelationIdDelegatingHandler>(true) // Good candidate for global
                .AddDelegatingHandler<UnifiedLoggingDelegatingHandler>(true); // Good candidate for global

            // Register custom aggregators
            services.AddSingleton<DownstreamServicesAggregator>(); // Ocelot will find it by type name in config

            // If using Ocelot.Cache.Distributed and have IDistributedCache registered (e.g. AddStackExchangeRedisCache)
            // Ocelot might pick it up automatically or might need explicit setup based on Ocelot version/config.
            // The .AddCacheManager() above is one way, another way might be configuring Ocelot's GlobalConfiguration in ocelot.json.

            return services;
        }
    }
}