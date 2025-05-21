using GraphQL;
using GraphQL.MicrosoftDI;
using GraphQL.Server; // For AddGraphQL extension
using GraphQL.SystemTextJson; // For AddSystemTextJson
using GraphQL.Types;
using GatewayService.GraphQL.Schemas;
using GatewayService.GraphQL.Types;
using GatewayService.GraphQL.Resolvers; // Assuming ExampleServiceResolver might be used/registered
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration; // Added for IConfiguration
using Microsoft.AspNetCore.Hosting; // For IWebHostEnvironment
using Microsoft.Extensions.Hosting; // For IHostEnvironment

namespace GatewayService.Extensions
{
    /// <summary>
    /// Extension methods for configuring GraphQL.NET.
    /// </summary>
    public static class GraphQLConfigurationExtensions
    {
        /// <summary>
        /// Configures GraphQL.NET services for the API Gateway.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="environment">The hosting environment.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddGraphQLServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            // Register GraphQL core services and System.Text.Json serializer
            services.AddGraphQL(builder => builder
                .AddSystemTextJson(options =>
                {
                    // Configure System.Text.Json options if needed
                    // options.PropertyNameCaseInsensitive = true;
                })
                .AddSchema<AppSchema>() // Registers AppSchema and tells GraphQL to use it
                .AddGraphTypes(typeof(AppSchema).Assembly) // Scans assembly for graph types
                .AddDataLoader() // Adds support for DataLoader pattern
                .AddErrorInfoProvider(opt => // Configure error info
                {
                    opt.ExposeExceptionStackTrace = environment.IsDevelopment();
                    opt.Expose всемرک کاStandard = true; // Exposes standard error fields like 'message', 'locations', 'path'
                    // opt.ExposeData = true; // Exposes the 'data' field in errors
                    // opt.ExposeCode = true; // Exposes the 'code' field in errors
                    // opt.ExposeCodes = true; // Exposes the 'codes' field in errors (plural)
                })
                // .AddAuthorization(options => // Configure GraphQL authorization if needed
                // {
                //    options.AddPolicy("AuthenticatedUserPolicy", p => p.RequireAuthenticatedUser());
                // })
                .AddServer(true) // Adds services required for GraphQL.Server (e.g. for subscriptions)
            );


            // Register schema and root types.
            // AddGraphQL().AddSchema<AppSchema>() handles this, but explicit registration can also be done.
            // services.AddSingleton<ISchema, AppSchema>(); // AppSchema will be resolved by AddSchema<AppSchema>()
            // services.AddTransient<RootQueryType>();
            // services.AddTransient<ExampleServiceDataType>();
            // ... other GraphQL types

            // Register any services/resolvers needed by your GraphQL types/fields
            // services.AddScoped<ExampleServiceResolver>(); // Example if you use separate resolver classes

            // Example: If GraphQL resolvers need HttpClient to call downstream services
            // Ensure named HttpClients are configured with Polly policies in PollyPolicyExtensions.cs
            // services.AddHttpClient("GraphQLDownstreamClient") // Configure this client in PollyPolicyExtensions
            //    .ConfigureHttpClient(client => { /* any specific http client config */ });

            return services;
        }
    }
}