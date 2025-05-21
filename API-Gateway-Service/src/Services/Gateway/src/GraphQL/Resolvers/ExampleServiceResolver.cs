using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GatewayService.Models; // For ExampleServiceData DTO

namespace GatewayService.GraphQL.Resolvers
{
    /// <summary>
    /// Contains logic to resolve fields by fetching data from downstream services.
    /// </summary>
    public class ExampleServiceResolver
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExampleServiceResolver> _logger;

        public ExampleServiceResolver(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ExampleServiceResolver> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Fetches example data from a downstream service.
        /// </summary>
        /// <param name="context">The GraphQL resolve field context.</param>
        /// <param name="id">The ID of the data to fetch.</param>
        /// <returns>An instance of ExampleServiceData or null if not found or an error occurs.</returns>
        public async Task<ExampleServiceData?> GetExampleDataAsync(IResolveFieldContext context, string id)
        {
            _logger.LogInformation("Attempting to resolve example data for ID: {Id}", id);

            // Example: Get base URL from configuration
            var exampleServiceBaseUrl = _configuration["ServiceEndpoints:ExampleServiceBaseUrl"]; // Assuming this key exists in appsettings.json
            if (string.IsNullOrEmpty(exampleServiceBaseUrl))
            {
                _logger.LogError("ExampleServiceBaseUrl is not configured.");
                // Optionally, communicate this error back through GraphQL errors
                context.Errors.Add(new GraphQL.ExecutionError("Service endpoint configuration error."));
                return null;
            }

            var httpClient = _httpClientFactory.CreateClient("DownstreamServiceClient"); // Named HttpClient

            try
            {
                // Simulate fetching data from a downstream service endpoint
                // In a real scenario, this URL would point to an actual microservice
                var requestUri = $"{exampleServiceBaseUrl.TrimEnd('/')}/api/data/{id}";
                _logger.LogDebug("Calling downstream service at {RequestUri}", requestUri);

                // This is a placeholder. A real service would return ExampleServiceData or similar.
                // We'll mock a response here for demonstration.
                if (id == "error-test")
                {
                     _logger.LogWarning("Simulating an error for ID: {Id}", id);
                     context.Errors.Add(new GraphQL.ExecutionError($"Simulated error fetching data for ID {id}."));
                     return null;
                }
                if (id == "not-found")
                {
                    _logger.LogInformation("Simulating not found for ID: {Id}", id);
                    return null; // Or throw an error that GraphQL can handle
                }

                // Mocked successful response:
                var mockData = new ExampleServiceData
                {
                    Id = id,
                    Name = $"Example Data for {id}",
                    Value = (float)Math.Round(new Random().NextDouble() * 1000, 2),
                    Timestamp = DateTime.UtcNow,
                    Tags = new List<string> { "example", "resolved", id }
                };
                _logger.LogInformation("Successfully resolved example data for ID: {Id}", id);
                return mockData;


                // Example of actual HTTP call (commented out for now as it requires a running service)
                /*
                var response = await httpClient.GetAsync(requestUri, context.CancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ExampleServiceData>(cancellationToken: context.CancellationToken);
                    _logger.LogInformation("Successfully fetched example data for ID: {Id}", id);
                    return data;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch example data for ID: {Id}. Status: {StatusCode}", id, response.StatusCode);
                    // Communicate error back to GraphQL client
                    context.Errors.Add(new GraphQL.ExecutionError($"Failed to fetch data. Status: {response.StatusCode}"));
                    return null;
                }
                */
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while fetching example data for ID: {Id}", id);
                context.Errors.Add(new GraphQL.ExecutionError("Network error occurred while fetching data.", ex));
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while resolving example data for ID: {Id}", id);
                context.Errors.Add(new GraphQL.ExecutionError("An unexpected error occurred.", ex));
                return null;
            }
        }

        // Add other resolver methods here for different GraphQL fields,
        // potentially calling other downstream services (Management, AI, Data, Integration)
        // For example:
        // public async Task<ManagementStatus> GetManagementStatusAsync(IResolveFieldContext context, string clientId) { ... }
        // public async Task<AiPrediction> GetAiPredictionAsync(IResolveFieldContext context, string tag) { ... }
        // public async Task<IEnumerable<TimeSeriesData>> GetHistoricalDataAsync(IResolveFieldContext context, string tagId, DateTime? startTime, DateTime? endTime) { ... }
    }
}