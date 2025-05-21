using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Transforms incoming responses from downstream services.
    /// Ocelot delegating handler responsible for transforming incoming responses from downstream services
    /// before they are sent back to the client. This can include changing response headers,
    /// modifying the response body, or adapting data formats.
    /// </summary>
    public class ResponseTransformationDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<ResponseTransformationDelegatingHandler> _logger;

        public ResponseTransformationDelegatingHandler(ILogger<ResponseTransformationDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("ResponseTransformationDelegatingHandler: Awaiting response from downstream for request to {RequestUri}", request.RequestUri);

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("ResponseTransformationDelegatingHandler: Transforming incoming response from {RequestUri}, Status: {StatusCode}", request.RequestUri, response.StatusCode);

            // Example: Add a custom response header
            response.Headers.TryAddWithoutValidation("X-Gateway-Processed-Response", "true");
            _logger.LogDebug("Added X-Gateway-Processed-Response header to the response.");

            // Example: Modify response body (careful with stream consumption and content types)
            // if (response.Content != null && response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType == "application/json")
            // {
            //     var originalContent = await response.Content.ReadAsStringAsync(cancellationToken);
            //     // TODO: Perform transformation on originalContent
            //     // For example, deserialize, modify, and re-serialize
            //     // var data = System.Text.Json.JsonSerializer.Deserialize<object>(originalContent);
            //     // // Modify data object
            //     // var transformedContent = System.Text.Json.JsonSerializer.Serialize(data);
            //     // response.Content = new StringContent(transformedContent, System.Text.Encoding.UTF8, "application/json");
            //     _logger.LogDebug("Transformed response body.");
            // }

            // TODO: Implement more specific response transformation logic as needed.
            // This could involve:
            // - Header manipulation (add, remove, modify)
            // - Body transformation (e.g., filtering fields, changing structure, data format conversion)
            // - Standardizing error responses

            return response;
        }
    }
}