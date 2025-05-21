using GatewayService.Aggregation;
using GatewayService.Middleware;
using GatewayService.Services; // For IDistributedCacheService and Ocelot.Cache.Distributed integration
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.Distributed; // For AddOcelotDistributedCache
using Ocelot.Provider.Polly; // For AddPolly

namespace GatewayService.Extensions
{
    /// <summary>
    /// Extension methods for configuring Ocelot.
    /// </summary>
    public static class OcelotConfigurationExtensions
    {
        /// <summary>
        /// Configures Ocelot API Gateway services and middleware.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration, which should include Ocelot's configuration at its root or a specified section.</param>
        /// <returns>An <see cref="IOcelotBuilder"/> that can be used to further configure Ocelot.</returns>
        public static IOcelotBuilder AddOcelotGateway(this IServiceCollection services, IConfiguration configuration)
        {
            // Register custom Ocelot delegating handlers as transient services
            // Ocelot will resolve them when they are specified in ocelot.json
            services.AddTransient<JwtValidationDelegatingHandler>();
            services.AddTransient<RateLimitingDelegatingHandler>();
            services.AddTransient<RequestTransformationDelegatingHandler>();
            services.AddTransient<ResponseTransformationDelegatingHandler>();
            services.AddTransient<UnifiedLoggingDelegatingHandler>();
            services.AddTransient<CorrelationIdDelegatingHandler>();

            var ocelotBuilder = services.AddOcelot(configuration); // Ocelot reads its config from ocelot.json by default if loaded into IConfiguration

            // Configure Ocelot Caching to use the registered IDistributedCacheService (e.g., Redis)
            // Requires IDistributedCacheService to be registered (e.g., services.AddSingleton<IDistributedCacheService, DistributedCacheService>();)
            // And IDistributedCache to be configured (e.g., services.AddStackExchangeRedisCache(...);)
            ocelotBuilder.AddOcelotDistributedCache(); // Uses the default IDistributedCache


            // Configure Ocelot with Polly for Quality of Service (QoS)
            // This allows Ocelot to use Polly policies defined in the IPolicyRegistry
            // (Policies should be added to the registry in PollyPolicyExtensions.cs)
            ocelotBuilder.AddPolly();

            // Register custom Ocelot aggregators
            // The aggregator type name ("DownstreamServicesAggregator") will be used in ocelot.json
            ocelotBuilder.AddSingletonDefinedAggregator<DownstreamServicesAggregator>();

            // Example of adding global delegating handlers if they should apply to all routes by default
            // These can also be configured per-route in ocelot.json
            // ocelotBuilder.AddDelegatingHandler<CorrelationIdDelegatingHandler>(global: true);
            // ocelotBuilder.AddDelegatingHandler<UnifiedLoggingDelegatingHandler>(global: true);

            return ocelotBuilder;
        }
    }
}