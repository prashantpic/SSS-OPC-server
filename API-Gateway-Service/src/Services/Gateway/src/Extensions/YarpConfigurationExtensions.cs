using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration; // For IReverseProxyBuilder

namespace GatewayService.Extensions
{
    /// <summary>
    /// Extension methods for configuring YARP.
    /// </summary>
    public static class YarpConfigurationExtensions
    {
        /// <summary>
        /// Configures YARP (Yet Another Reverse Proxy) for the API Gateway.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration, expected to contain a "Yarp" section.</param>
        /// <returns>The <see cref="IReverseProxyBuilder"/> so that additional YARP calls can be chained.</returns>
        public static IReverseProxyBuilder AddYarpReverseProxy(this IServiceCollection services, IConfiguration configuration)
        {
            var yarpConfigSection = configuration.GetSection("Yarp");
            if (!yarpConfigSection.Exists())
            {
                // Log this warning or throw if YARP is critical
                Console.WriteLine("Warning: YARP configuration section 'Yarp' not found. YARP will not function correctly.");
                // Or throw new ApplicationException("YARP configuration section 'Yarp' must be present.");
            }

            var reverseProxyBuilder = services.AddReverseProxy();

            // Load YARP configuration from the "Yarp" section of IConfiguration
            // This assumes yarp.json content is loaded under a "Yarp" key in the main configuration,
            // or yarp.json itself is structured as { "Yarp": { "Routes": ..., "Clusters": ... } }
            reverseProxyBuilder.LoadFromConfig(yarpConfigSection);

            // Add custom YARP transforms if any are developed.
            // Example:
            // reverseProxyBuilder.AddTransforms(transformBuilderContext =>
            // {
            //    transformBuilderContext.AddRequestHeader("X-Custom-Yarp-Header", "YarpWasHere");
            // });

            return reverseProxyBuilder;
        }
    }
}