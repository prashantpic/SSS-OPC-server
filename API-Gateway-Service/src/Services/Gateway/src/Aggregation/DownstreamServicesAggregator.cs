using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Multiplexer; // Correct namespace for IDefinedAggregator
using Ocelot.Middleware; // For DownstreamResponse
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Aggregation
{
    /// <summary>
    /// Aggregates responses from multiple downstream services for Ocelot.
    /// Custom Ocelot IDefinedAggregator implementation for scenarios requiring aggregation of responses
    /// from multiple downstream services. This class contains logic to orchestrate calls and merge
    /// their results into a unified response payload.
    /// </summary>
    public class DownstreamServicesAggregator : IDefinedAggregator
    {
        private readonly ILogger<DownstreamServicesAggregator> _logger;

        public DownstreamServicesAggregator(ILogger<DownstreamServicesAggregator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Aggregates responses from multiple downstream services.
        /// </summary>
        /// <param name="responses">A list of DownstreamResponse objects from the services defined in Ocelot's aggregation configuration.</param>
        /// <returns>A DownstreamResponse containing the aggregated result.</returns>
        public async Task<DownstreamResponse> Aggregate(List<DownstreamResponse> responses, HttpContext httpContext)
        {
            _logger.LogInformation("DownstreamServicesAggregator: Starting aggregation of {Count} responses.", responses.Count);

            var aggregatedData = new Dictionary<string, JsonElement>();
            var responseHeaders = new List<Header>();
            HttpStatusCode finalStatusCode = HttpStatusCode.OK; // Default, can be adjusted based on responses

            foreach (var downstreamResponse in responses)
            {
                var routeKey = httpContext.Items.FirstOrDefault(x => x.Value.Equals(downstreamResponse)).Key?.ToString(); // Ocelot adds route key to HttpContext.Items for the aggregator
                if (string.IsNullOrEmpty(routeKey))
                {
                    // Fallback or generate a unique key if Ocelot doesn't provide it as expected
                    routeKey = $"response_{System.Guid.NewGuid().ToString("N").Substring(0, 6)}"; 
                     _logger.LogWarning("Could not determine route key for a downstream response during aggregation. Using generated key: {RouteKey}", routeKey);
                }


                if (downstreamResponse.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("Downstream service for key '{RouteKey}' responded with status {StatusCode}. Content might be skipped or handled as error.", 
                        routeKey, downstreamResponse.StatusCode);
                    // Optionally, change finalStatusCode or include error information in aggregatedData
                    if(finalStatusCode == HttpStatusCode.OK) finalStatusCode = downstreamResponse.StatusCode; // Or pick the "worst" status
                }
                
                try
                {
                    var contentStream = await downstreamResponse.Content.ReadAsStreamAsync();
                    if (contentStream.Length > 0)
                    {
                        var contentElement = await JsonSerializer.DeserializeAsync<JsonElement>(contentStream);
                        aggregatedData[routeKey] = contentElement;
                        _logger.LogDebug("Successfully deserialized and added content for route key: {RouteKey}", routeKey);
                    }
                    else
                    {
                        _logger.LogDebug("Response content for route key '{RouteKey}' is empty.", routeKey);
                        // aggregatedData[routeKey] = JsonDocument.Parse("{}").RootElement; // Add empty object if needed
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize JSON content from downstream service for route key '{RouteKey}'. Status: {StatusCode}", 
                        routeKey, downstreamResponse.StatusCode);
                    // Add error info to aggregatedData or handle as needed
                    aggregatedData[routeKey] = JsonDocument.Parse($"{{\"error\":\"Failed to parse response from {routeKey}\", \"statusCode\":{(int)downstreamResponse.StatusCode}}}").RootElement;
                    if(finalStatusCode == HttpStatusCode.OK) finalStatusCode = HttpStatusCode.MultiStatus; // Or another appropriate status
                }
                catch (Exception ex)
                {
                     _logger.LogError(ex, "Generic error processing response from downstream service for route key '{RouteKey}'. Status: {StatusCode}", 
                        routeKey, downstreamResponse.StatusCode);
                    aggregatedData[routeKey] = JsonDocument.Parse($"{{\"error\":\"Error processing response from {routeKey}\", \"statusCode\":{(int)downstreamResponse.StatusCode}}}").RootElement;
                     if(finalStatusCode == HttpStatusCode.OK) finalStatusCode = HttpStatusCode.MultiStatus;
                }
                
                // Aggregate headers if needed - this example just takes from the first successful response or creates new ones
                if (responseHeaders.Count == 0 && downstreamResponse.StatusCode == HttpStatusCode.OK)
                {
                    foreach (var header in downstreamResponse.Headers)
                    {
                        responseHeaders.Add(new Header(header.Key, header.Values));
                    }
                }
            }

            var aggregatedJson = JsonSerializer.Serialize(aggregatedData);
            var stringContent = new StringContent(aggregatedJson, System.Text.Encoding.UTF8, "application/json");
            
            // Ensure some basic headers if none were aggregated
            if(!responseHeaders.Any(h => h.Key.Equals("Content-Type", System.StringComparison.OrdinalIgnoreCase)))
            {
                responseHeaders.Add(new Header("Content-Type", new List<string>{"application/json"}));
            }

            _logger.LogInformation("Aggregation complete. Final status code: {StatusCode}. Returning aggregated response.", finalStatusCode);
            return new DownstreamResponse(stringContent, finalStatusCode, responseHeaders, "OK");
        }
    }
}