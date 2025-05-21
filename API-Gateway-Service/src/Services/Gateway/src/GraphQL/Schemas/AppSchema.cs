using GatewayService.GraphQL.Types;
using GraphQL.Types;
using GraphQL.Utilities; // For GetRequiredService
using System;

namespace GatewayService.GraphQL.Schemas
{
    /// <summary>
    /// Root GraphQL schema definition.
    /// Defines the main GraphQL schema for the API Gateway using GraphQL.NET.
    /// It serves as the entry point for all GraphQL operations, linking together
    /// Query, Mutation, and Subscription root types.
    /// </summary>
    public class AppSchema : Schema
    {
        public AppSchema(IServiceProvider provider) : base(provider)
        {
            Query = provider.GetRequiredService<RootQueryType>();
            // Mutation = provider.GetRequiredService<RootMutationType>(); // Uncomment if you have mutations
            // Subscription = provider.GetRequiredService<RootSubscriptionType>(); // Uncomment if you have subscriptions

            // Example: If you need to apply field middleware globally or based on conventions
            // FieldMiddleware.Use(new InstrumentFieldsMiddleware());
            // Or
            // FieldMiddleware.Use(typeof(MyCustomFieldMiddleware));
        }
    }
}