using GraphQL;
using GraphQL.Types;
using GatewayService.GraphQL.Resolvers;
using GatewayService.GraphQL.Types; // Assuming ExampleServiceDataType is here
using GatewayService.Models; // For ExampleServiceData DTO

namespace GatewayService.GraphQL.Types
{
    /// <summary>
    /// Defines the root query type for the GraphQL schema.
    /// This class aggregates all queryable fields, each associated with a resolver
    /// that fetches data, often by calling appropriate downstream microservices.
    /// </summary>
    public class RootQueryType : ObjectGraphType
    {
        public RootQueryType(ExampleServiceResolver exampleServiceResolver)
        {
            Name = "Query";
            Description = "The root query for accessing system data.";

            Field<StringGraphType>(
                name: "managementStatus",
                description: "Gets the status of a management client.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "clientId", Description = "The ID of the client" }
                ),
                resolve: context =>
                {
                    // Placeholder: In a real scenario, call a resolver or service
                    // that interacts with the Management-Service.
                    var clientId = context.GetArgument<string>("clientId");
                    // return exampleServiceResolver.GetManagementStatusAsync(context, clientId);
                    return $"Status for client {clientId}: OK (Placeholder)";
                });

            Field<AiPredictionType>(
                name: "aiPrediction",
                description: "Gets an AI prediction for a specific tag.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "tag", Description = "The tag for which to get a prediction" }
                ),
                resolve: context =>
                {
                    // Placeholder: In a real scenario, call a resolver or service
                    // that interacts with the AI-Processing-Service.
                    var tag = context.GetArgument<string>("tag");
                    // return exampleServiceResolver.GetAiPredictionAsync(context, tag);
                    return new AiPrediction { Tag = tag, Prediction = (float)System.Math.Round(new System.Random().NextDouble() * 100, 2) };
                });

            Field<ListGraphType<TimeSeriesDataType>>(
                name: "historicalData",
                description: "Gets historical time series data for a tag.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "tagId", Description = "The ID of the tag" },
                    new QueryArgument<DateTimeGraphType> { Name = "startTime", Description = "Start time for the data range" },
                    new QueryArgument<DateTimeGraphType> { Name = "endTime", Description = "End time for the data range" }
                ),
                resolve: context =>
                {
                    // Placeholder: In a real scenario, call a resolver or service
                    // that interacts with the Data-Service.
                    var tagId = context.GetArgument<string>("tagId");
                    // return exampleServiceResolver.GetHistoricalDataAsync(context, tagId, startTime, endTime);
                    return new List<TimeSeriesData>
                    {
                        new TimeSeriesData { Timestamp = System.DateTime.UtcNow.AddHours(-1), Value = (float)System.Math.Round(new System.Random().NextDouble() * 10, 2) },
                        new TimeSeriesData { Timestamp = System.DateTime.UtcNow, Value = (float)System.Math.Round(new System.Random().NextDouble() * 10, 2) }
                    };
                });

            // Example using the ExampleServiceResolver and ExampleServiceDataType
            Field<ExampleServiceDataType>(
                name: "exampleServiceData",
                description: "Fetches example data from a downstream service.",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "id", Description = "The ID for the example data" }
                ),
                resolve: async context =>
                {
                    var id = context.GetArgument<string?>("id");
                    return await exampleServiceResolver.GetExampleDataAsync(context, id ?? "default-id");
                });
        }
    }

    // Placeholder types referenced by RootQueryType, matching graphql.schema.graphql
    // In a real application, these would be more fleshed out ObjectGraphTypes.

    public class ManagementStatusType : ObjectGraphType<ManagementStatus>
    {
        public ManagementStatusType()
        {
            Name = "ManagementStatus";
            Description = "Represents the status of a managed client.";
            Field(x => x.ClientId, type: typeof(IdGraphType)).Description("The ID of the client.");
            Field(x => x.Status).Description("The current status of the client.");
        }
    }

    public class AiPredictionType : ObjectGraphType<AiPrediction>
    {
        public AiPredictionType()
        {
            Name = "AiPrediction";
            Description = "Represents an AI-generated prediction.";
            Field(x => x.Tag).Description("The tag for which the prediction was made.");
            Field(x => x.Prediction, type: typeof(FloatGraphType)).Description("The predicted value.");
        }
    }

    public class TimeSeriesDataType : ObjectGraphType<TimeSeriesData>
    {
        public TimeSeriesDataType()
        {
            Name = "TimeSeriesData";
            Description = "Represents a single point of time series data.";
            Field(x => x.Timestamp, type: typeof(DateTimeGraphType)).Description("The timestamp of the data point.");
            Field(x => x.Value, type: typeof(FloatGraphType)).Description("The value of the data point.");
        }
    }
}