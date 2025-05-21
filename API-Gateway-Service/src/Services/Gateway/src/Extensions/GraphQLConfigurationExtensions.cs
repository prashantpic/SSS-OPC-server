using GatewayService.GraphQL.Resolvers;
using GatewayService.GraphQL.Schemas;
using GatewayService.GraphQL.Types; // Assuming your GraphQL types are here
using GraphQL.Server; // For AddGraphQL extension
using GraphQL.SystemTextJson; // For System.Text.Json serialization
using GraphQL; // For IDocumentExecuter and such

namespace GatewayService.Extensions
{
    public static class GraphQLConfigurationExtensions
    {
        public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
        {
            // Add GraphQL services
            services.AddGraphQL(builder => builder
                .AddSchema<AppSchema>() // Register your main schema
                .AddSystemTextJson()    // Use System.Text.Json for serialization (recommended for ASP.NET Core)
                .AddErrorInfoProvider(opt => opt.ExposeExceptionStackTrace = true) // Configure error details (consider for dev only)
                .AddGraphTypes(typeof(AppSchema).Assembly) // Discover and register all graph types in the assembly
                .AddDataLoader() // For data loader support (efficient batching)
                .AddUserContextBuilder(httpContext => new GraphQLUserContext { User = httpContext.User }) // Example user context
                // .AddAuthorization(options => /* Configure GraphQL authorization rules here */) // If using GraphQL.NET authorization
            );


            // Register your GraphQL resolvers, types, and schema
            // Graph types are often discovered by .AddGraphTypes, but resolvers might need explicit registration
            // depending on how they are structured and injected.
            services.AddScoped<AppSchema>(); // Or Singleton depending on your schema's lifetime needs
            services.AddScoped<RootQueryType>();
            // Add other GraphQL types if they are not automatically discovered or have complex dependencies
            services.AddScoped<ExampleServiceDataType>();
            services.AddScoped<ManagementStatusType>();
            services.AddScoped<AiPredictionType>();
            services.AddScoped<TimeSeriesDataType>();


            // Register resolvers - these contain the logic to fetch data
            services.AddScoped<ExampleServiceResolver>();
            services.AddScoped<ManagementServiceResolver>();
            services.AddScoped<AIServiceResolver>();
            services.AddScoped<DataServiceResolver>();


            // If you are using GraphQL.Server.Ui.Playground or Altair for a UI
            // services.AddGraphQLPlayground(options => { options.Path = "/ui/playground"; });
            // services.AddGraphQLAltair(options => { options.Path = "/ui/altair"; });

            return services;
        }
    }

    // Example User Context class
    public class GraphQLUserContext : Dictionary<string, object>
    {
        public System.Security.Claims.ClaimsPrincipal User { get; set; }
    }
}