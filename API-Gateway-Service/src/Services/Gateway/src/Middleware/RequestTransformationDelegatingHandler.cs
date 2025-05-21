using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayService.Middleware
{
    /// <summary>
    /// Transforms outgoing requests to downstream services.
    /// Ocelot delegating handler responsible for transforming outgoing requests to downstream services.
    /// This can include changing request headers, modifying the request body, or translating between protocols.
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
            _logger.LogInformation("RequestTransformationDelegatingHandler: Transforming outgoing request to {RequestUri}", request.RequestUri);

            // Example: Add a custom header
            request.Headers.TryAddWithoutValidation("X-Custom-Gateway-Header", "TransformedValue");
            _logger.LogDebug("Added X-Custom-Gateway-Header to outgoing request.");

            // Example: Modify query parameters (more complex, requires parsing and rebuilding URI)
            // var uriBuilder = new UriBuilder(request.RequestUri);
            // var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            // query["newParam"] = "newValue";
            // uriBuilder.Query = query.ToString();
            // request.RequestUri = uriBuilder.Uri;

            // Example: Modify request body (careful with stream consumption)
            // if (request.Content != null)
            // {
            //     var originalContent = await request.Content.ReadAsStringAsync(cancellationToken);
            //     // TODO: Perform transformation on originalContent
            //     var transformedContent = originalContent.ToUpperInvariant(); // Simple example
            //     request.Content = new StringContent(transformedContent, System.Text.Encoding.UTF8, "application/json");
            //     _logger.LogDebug("Transformed request body.");
            // }

            // TODO: Implement more specific request transformation logic as needed.
            // This could involve:
            // - Header manipulation (add, remove, modify)
            // - Query parameter manipulation
            // - Body transformation (e.g., JSON to XML, field renaming)
            // - Adding gRPC metadata if routing to a gRPC service (though YARP is better for gRPC)

            return await base.SendAsync(request, cancellationToken);
        }
    }
}