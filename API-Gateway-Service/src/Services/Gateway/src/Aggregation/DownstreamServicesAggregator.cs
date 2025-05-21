using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json; // Using Newtonsoft.Json for dynamic JSON manipulation convenience
using Newtonsoft.Json.Linq;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GatewayService.Aggregation
{
    /// <summary>
    /// Aggregates responses from multiple downstream services for Ocelot.
    /// Custom Ocelot IDefinedAggregator implementation for scenarios requiring aggregation of responses 
    /// from multiple downstream services. This class contains logic to orchestrate parallel or sequential 
    /// calls and merge their results into a unified response payload.
    /// </summary>
    public class DownstreamServicesAggregator : IDefinedAggregator
    {
        private readonly ILogger<DownstreamServicesAggregator> _logger;

        public DownstreamServicesAggregator(ILogger<DownstreamServicesAggregator> logger)
        {
            _logger = logger;
        }

        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responsesContexts)
        {
            _logger.LogInformation("Starting aggregation for {Count} downstream responses.", responsesContexts.Count);

            var aggregatedData = new JObject();
            var headers = new List<Header>();
            HttpStatusCode finalStatusCode = HttpStatusCode.OK; // Default, might change based on responses

            foreach (var context in responsesContexts)
            {
                var downstreamResponse = context.Items.DownstreamResponse();
                if (downstreamResponse == null)
                {
                    _logger.LogWarning("DownstreamResponse is null in one of the HttpContext items. Skipping.");
                    finalStatusCode = HttpStatusCode.InternalServerError; // Or handle more gracefully
                    continue;
                }

                // Use RouteKey from Ocelot configuration if available (requires ocelot.json setup for aggregates)
                // This key helps in structuring the aggregated JSON.
                var routeKey = context.Items.DownstreamRoute().RouteKey ?? $"response_{responsesContexts.IndexOf(context)}";
                
                _logger.LogDebug("Processing response for route key: {RouteKey}, Status: {StatusCode}", routeKey, downstreamResponse.StatusCode);

                if (downstreamResponse.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("Downstream service for route key {RouteKey} returned non-OK status: {StatusCode}", routeKey, downstreamResponse.StatusCode);
                    // Decide aggregation strategy for errors:
                    // - Fail entire aggregation
                    // - Include error information in the aggregated response
                    // - Ignore failed service
                    // For this example, we'll try to include content if available, otherwise mark as error
                    finalStatusCode = (finalStatusCode == HttpStatusCode.OK) ? downstreamResponse.StatusCode : finalStatusCode; // Keep first error or a general error
                }
                
                try
                {
                    var contentStream = await downstreamResponse.Content.ReadAsStreamAsync();
                    if (contentStream.Length > 0)
                    {
                        contentStream.Position = 0;
                        using (var sr = new StreamReader(contentStream))
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            // Attempt to parse as JToken (JObject or JArray)
                            var tokenData = await JToken.ReadFromAsync(jsonTextReader);
                            aggregatedData[routeKey] = tokenData;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Response content for {RouteKey} is empty.", routeKey);
                        aggregatedData[routeKey] = new JObject { ["error"] = $"Empty response from {routeKey}", ["statusCode"] = (int)downstreamResponse.StatusCode };
                    }
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, "Failed to parse JSON content from downstream service {RouteKey}.", routeKey);
                    aggregatedData[routeKey] = new JObject { ["error"] = $"Invalid JSON response from {routeKey}", ["statusCode"] = (int)downstreamResponse.StatusCode };
                    finalStatusCode = (finalStatusCode == HttpStatusCode.OK) ? HttpStatusCode.InternalServerError : finalStatusCode;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing content from downstream service {RouteKey}.", routeKey);
                    aggregatedData[routeKey] = new JObject { ["error"] = $"Error processing response from {routeKey}", ["statusCode"] = (int)downstreamResponse.StatusCode };
                    finalStatusCode = (finalStatusCode == HttpStatusCode.OK) ? HttpStatusCode.InternalServerError : finalStatusCode;
                }

                // Collect headers (e.g., from the first successful response or merge them)
                // For simplicity, taking headers from the first context if none are collected yet
                if (!headers.Any() && downstreamResponse.Headers.Any())
                {
                    headers.AddRange(downstreamResponse.Headers);
                }
            }

            _logger.LogInformation("Aggregation complete. Final status code will be: {StatusCode}", finalStatusCode);

            var stringContent = new StringContent(aggregatedData.ToString(Formatting.None), Encoding.UTF8, "application/json");
            
            // Ensure standard headers like Content-Type are set correctly
            headers.RemoveAll(h => h.Key.Equals("Content-Type", System.StringComparison.OrdinalIgnoreCase) || 
                                   h.Key.Equals("Content-Length", System.StringComparison.OrdinalIgnoreCase));
            headers.Add(new Header("Content-Type", new[] { "application/json" }));
            // Content-Length will be set automatically by HttpClient

            return new DownstreamResponse(stringContent, finalStatusCode, headers, "Aggregated");
        }
    }
}