using GatewayService.Models; // Assuming ExampleDataDto is in Models namespace
using GatewayService.GraphQL.Types; // For ExampleDataDto
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GatewayService.GraphQL.Resolvers
{
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

        public async Task<ExampleDataDto?> GetExampleDataAsync(string id)
        {
            _logger.LogInformation("Resolver: Fetching example data for ID: {Id}", id);

            var client = _httpClientFactory.CreateClient("DownstreamServiceClient"); // Assumes a named HttpClient configured with Polly policies

            // Example: Fetching from a hypothetical "ManagementService"
            // The actual service endpoint should be configurable.
            var serviceBaseUrl = _configuration["ServiceEndpoints:ManagementService"];
            if (string.IsNullOrEmpty(serviceBaseUrl))
            {
                _logger.LogError("Resolver: ManagementService endpoint is not configured.");
                throw new InvalidOperationException("ManagementService endpoint is not configured.");
            }

            var requestUri = $"{serviceBaseUrl.TrimEnd('/')}/api/exampledata/{id}"; // Adjust endpoint as needed

            try
            {
                var response = await client.GetAsync(requestUri);
                response.EnsureSuccessStatusCode(); // Throws HttpRequestException for non-success codes

                var data = await response.Content.ReadFromJsonAsync<ExampleDataDto>();
                _logger.LogInformation("Resolver: Successfully fetched example data for ID: {Id}", id);
                return data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Resolver: HTTP request failed while fetching example data for ID: {Id} from {RequestUri}", id, requestUri);
                // Specific error handling or re-throwing can be done here.
                // For GraphQL, this exception will likely be caught by the field resolver in RootQueryType
                // and added to context.Errors.
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resolver: An unexpected error occurred while fetching example data for ID: {Id}", id);
                throw; // Re-throw to be handled by GraphQL execution
            }
        }

        // Add other resolver methods here for different data types or services
        // For example:
        // public async Task<List<ManagementClientStatusDto>> GetClientStatusesAsync() { ... }
        // public async Task<PredictionResultDto> GetAiPredictionAsync(InputDataDto input) { ... }
    }
}