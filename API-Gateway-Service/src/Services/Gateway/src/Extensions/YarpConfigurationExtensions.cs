using Yarp.ReverseProxy.Configuration;

namespace GatewayService.Extensions
{
    public static class YarpConfigurationExtensions
    {
        public static IServiceCollection AddYarpServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Load YARP configuration from the "Yarp" section of appsettings.json or yarp.json
            services.AddReverseProxy()
                .LoadFromConfig(configuration.GetSection("Yarp"));
            // In Program.cs, ensure yarp.json is loaded into IConfiguration:
            // builder.Configuration.AddJsonFile("yarp.json", optional: false, reloadOnChange: true);

            // Example of custom configuration provider if needed:
            // services.AddSingleton<IProxyConfigProvider, CustomProxyConfigProvider>();

            // Example of adding custom transforms programmatically if not fully covered by config:
            // services.AddSingleton<IYarpRateLimiterPolicy, CustomRateLimiter>();
            // services.AddTransformFactory<CustomTransformFactory>();

            return services;
        }
    }
}