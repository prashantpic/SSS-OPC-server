using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
// using System.Text; // For body manipulation
// using System.IO;   // For body manipulation

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
            var httpContext = request.Properties["HttpContext"] as HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is not available in HttpRequestMessage properties for response transformation.");
                return await base.SendAsync(request, cancellationToken);
            }
            
            var downstreamRoute = httpContext.Items.DownstreamRoute();
            _logger.LogDebug("Preparing for response transformations for route: {UpstreamPath}", downstreamRoute.UpstreamPathTemplate.OriginalValue);

            // Get the response from the downstream service
            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogDebug("Applying response transformations for route: {UpstreamPath}, Status: {StatusCode}", 
                downstreamRoute.UpstreamPathTemplate.OriginalValue, response.StatusCode);

            // Example: Add a custom header to the response
            response.Headers.TryAddWithoutValidation("X-Gateway-Transform", "ResponseTransformed");
            _logger.LogInformation("Added 'X-Gateway-Transform' header to response for {Path}", request.RequestUri);

            // Example: Remove an internal header
            if (response.Headers.Contains("X-Internal-Service-Info"))
            {
                response.Headers.Remove("X-Internal-Service-Info");
                _logger.LogInformation("Removed 'X-Internal-Service-Info' header from response for {Path}", request.RequestUri);
            }

            // Example: Modify response status code (use with caution)
            // if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            // {
            //     // response.StatusCode = System.Net.HttpStatusCode.NoContent; // Example change
            //     _logger.LogInformation("Changed response status code for {Path}", request.RequestUri);
            // }

            // Example: Altering response body (complex and requires careful handling)
            // This involves reading the stream, modifying content, and replacing the response.Content.
            // It's performance-sensitive and can break streaming.
            // if (response.Content != null && response.IsSuccessStatusCode)
            // {
            //     var mediaType = response.Content.Headers.ContentType?.MediaType;
            //     if (mediaType == "application/json")
            //     {
            //         _logger.LogInformation("Attempting to transform response body for {Path}", request.RequestUri);
            //         // IMPORTANT: Reading response.Content consumes it. It must be buffered and replaced.
            //         // var originalContent = await response.Content.ReadAsStringAsync(cancellationToken);
            //         // var modifiedContent = originalContent.Replace("internal_field", "public_field"); // Example transformation
            //         // response.Content = new StringContent(modifiedContent, Encoding.UTF8, mediaType);
            //         // _logger.LogInformation("Response body transformed.");
            //     }
            // }
            
            // Log transformations applied
            _logger.LogDebug("Response transformations complete for route: {UpstreamPath}", downstreamRoute.UpstreamPathTemplate.OriginalValue);

            return response;
        }
    }
}