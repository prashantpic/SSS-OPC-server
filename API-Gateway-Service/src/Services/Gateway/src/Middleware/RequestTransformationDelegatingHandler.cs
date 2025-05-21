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
    /// Transforms outgoing requests to downstream services.
    /// Ocelot delegating handler responsible for transforming outgoing requests to downstream services. 
    /// This can include changing request headers, modifying the request body, 
    /// or translating between protocols (e.g., adding specific gRPC metadata if Ocelot is routing to a gRPC service).
    /// </summary>
    public class RequestTransformationDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<RequestTransformationDelegatingHandler> _logger;

        public RequestTransformationDelegatingHandler(ILogger<RequestTransformationDelegatingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = request.Properties["HttpContext"] as HttpContext;
            if (httpContext == null)
            {
                _logger.LogError("HttpContext is not available in HttpRequestMessage properties.");
                // Potentially return an error or let it pass, depending on policy
                return await base.SendAsync(request, cancellationToken);
            }

            var downstreamRoute = httpContext.Items.DownstreamRoute();
            _logger.LogDebug("Applying request transformations for route: {UpstreamPath}", downstreamRoute.UpstreamPathTemplate.OriginalValue);

            // Example: Add a custom header
            request.Headers.TryAddWithoutValidation("X-Gateway-Transform", "RequestTransformed");
            _logger.LogInformation("Added 'X-Gateway-Transform' header to request for {Path}", request.RequestUri);

            // Example: Modify path (Ocelot usually handles this via DownstreamPathTemplate)
            // This would be for more dynamic path changes not covered by Ocelot's static config.
            // var originalPath = request.RequestUri.AbsolutePath;
            // if (originalPath.Contains("/old/"))
            // {
            //     var newPath = originalPath.Replace("/old/", "/new/");
            //     request.RequestUri = new Uri(request.RequestUri, newPath);
            //     _logger.LogInformation("Rewrote path from {OriginalPath} to {NewPath}", originalPath, newPath);
            // }

            // Example: Add gRPC metadata if routing to a gRPC service (conceptual)
            // This requires knowing if the downstream is gRPC. Ocelot itself doesn't inherently know,
            // but YARP handles gRPC routing. If Ocelot routes HTTP to a gRPC backend (less common),
            // headers might need to be set. For YARP, transforms are configured in yarp.json.
            if (downstreamRoute.DownstreamScheme?.ToLower() == "grpc" || downstreamRoute.DownstreamScheme?.ToLower() == "h2c") // Hypothetical check
            {
                 request.Headers.TryAddWithoutValidation("grpc-custom-header", "value-from-gateway");
                 _logger.LogInformation("Added gRPC custom header for potential gRPC downstream service.");
            }
            
            // Example: Altering request body (complex and requires careful handling)
            // This involves reading the stream, modifying content, and replacing the request.Content.
            // It's performance-sensitive and can break streaming.
            // if (request.Content != null && request.Method == HttpMethod.Post)
            // {
            //     _logger.LogInformation("Attempting to transform request body for {Path}", request.RequestUri);
            //     // IMPORTANT: Reading request.Content consumes it. It must be buffered and replaced.
            //     // var originalContent = await request.Content.ReadAsStringAsync(cancellationToken);
            //     // var modifiedContent = originalContent.Replace("foo", "bar"); // Example transformation
            //     // request.Content = new StringContent(modifiedContent, Encoding.UTF8, request.Content.Headers.ContentType?.MediaType ?? "application/json");
            //     // _logger.LogInformation("Request body transformed.");
            // }


            // Log transformations applied
            _logger.LogDebug("Request transformations complete for route: {UpstreamPath}", downstreamRoute.UpstreamPathTemplate.OriginalValue);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}