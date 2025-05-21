using GraphQL;
using GraphQL.Types;
using GatewayService.GraphQL.Resolvers;
using GatewayService.GraphQL.Types; // Assuming ExampleServiceDataType and related DTOs are here or in Models
using GatewayService.Models; // For ExampleDataDto
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GatewayService.GraphQL.Types
{
    public class RootQueryType : ObjectGraphType
    {
        public RootQueryType(
            ExampleServiceResolver resolver,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<RootQueryType> logger)
        {
            Name = "Query";
            Description = "The root query type for all GraphQL queries.";

            Field<ExampleServiceDataType, ExampleDataDto>()
                .Name("getExampleDataById")
                .Description("Retrieves example data by its ID.")
                .Argument<NonNullGraphType<IdGraphType>>("id", "The ID of the example data.")
                .ResolveAsync(async context =>
                {
                    var id = context.GetArgument<string>("id");
                    logger.LogInformation("GraphQL: Executing getExampleDataById for ID: {Id}", id);
                    try
                    {
                        // Example of directly using HttpClientFactory if resolver is simple
                        // var client = httpClientFactory.CreateClient("DownstreamServiceClient");
                        // var managementServiceBaseUrl = configuration["ServiceEndpoints:ManagementService"];
                        // var response = await client.GetAsync($"{managementServiceBaseUrl}/api/example/{id}");
                        // response.EnsureSuccessStatusCode();
                        // return await response.Content.ReadFromJsonAsync<ExampleDataDto>();

                        // Using the dedicated resolver class
                        return await resolver.GetExampleDataAsync(id);
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.LogError(ex, "GraphQL: HTTP request failed while fetching example data for ID: {Id}", id);
                        context.Errors.Add(new ExecutionError($"Failed to fetch data: {ex.Message}", ex));
                        return null;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "GraphQL: An error occurred while fetching example data for ID: {Id}", id);
                        context.Errors.Add(new ExecutionError("An internal error occurred.", ex));
                        return null;
                    }
                });

            Field<ListGraphType<ManagementClientStatusType>>()
                .Name("listClients")
                .Description("Retrieves a list of management client statuses.")
                .ResolveAsync(async context =>
                {
                    logger.LogInformation("GraphQL: Executing listClients query.");
                    var client = httpClientFactory.CreateClient("DownstreamServiceClient"); // Assumes a named HttpClient
                    var managementServiceBaseUrl = configuration["ServiceEndpoints:ManagementService"];
                    if (string.IsNullOrEmpty(managementServiceBaseUrl))
                    {
                        context.Errors.Add(new ExecutionError("ManagementService endpoint is not configured."));
                        return null;
                    }

                    try
                    {
                        var response = await client.GetAsync($"{managementServiceBaseUrl}/clients/status"); // Example endpoint
                        response.EnsureSuccessStatusCode();
                        var clientStatuses = await response.Content.ReadFromJsonAsync<List<ManagementClientStatusDto>>();
                        return clientStatuses;
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.LogError(ex, "GraphQL: HTTP request failed while listing clients.");
                        context.Errors.Add(new ExecutionError($"Failed to list clients: {ex.Message}", ex));
                        return new List<ManagementClientStatusDto>();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "GraphQL: An error occurred while listing clients.");
                        context.Errors.Add(new ExecutionError("An internal error occurred while listing clients.", ex));
                        return new List<ManagementClientStatusDto>();
                    }
                });

            // Placeholder for AI Prediction query based on graphql.schema.graphql
            Field<PredictionResultType>()
                .Name("getAiPrediction")
                .Description("Retrieves an AI prediction based on input data.")
                .Argument<NonNullGraphType<InputDataInputType>>("data", "The input data for the prediction.")
                .ResolveAsync(async context =>
                {
                    var inputData = context.GetArgument<InputDataDto>("data");
                    logger.LogInformation("GraphQL: Executing getAiPrediction query.");
                    var client = httpClientFactory.CreateClient("DownstreamServiceClient");
                    var aiServiceBaseUrl = configuration["ServiceEndpoints:AiService"];
                     if (string.IsNullOrEmpty(aiServiceBaseUrl))
                    {
                        context.Errors.Add(new ExecutionError("AIService endpoint is not configured."));
                        return null;
                    }
                    try
                    {
                        // Simulate calling AI service
                        // var response = await client.PostAsJsonAsync($"{aiServiceBaseUrl}/predict", inputData);
                        // response.EnsureSuccessStatusCode();
                        // return await response.Content.ReadFromJsonAsync<PredictionResultDto>();

                        // Placeholder implementation
                        await Task.Delay(100); // Simulate network latency
                        return new PredictionResultDto { PredictionId = Guid.NewGuid().ToString(), Score = new Random().NextDouble(), Details = "Sample prediction details" };
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "GraphQL: An error occurred while getting AI prediction.");
                        context.Errors.Add(new ExecutionError("An internal error occurred while getting AI prediction.", ex));
                        return null;
                    }
                });
        }
    }
}