using Yarp.ReverseProxy.Transforms;

namespace Gateways.Api.Transforms;

/// <summary>
/// Provides custom request transformations, such as adding a correlation ID for distributed tracing.
/// </summary>
public class CustomTransformProvider : ITransformProvider
{
    /// <summary>
    /// Validates route-specific transform configurations.
    /// </summary>
    /// <param name="context">The validation context.</param>
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // No custom validation logic is needed for this simple transform.
        // This method could be used to validate transform configuration from appsettings.json.
    }

    /// <summary>
    /// Applies transformations to the request.
    /// </summary>
    /// <param name="context">The transform builder context.</param>
    public void Apply(TransformBuilderContext context)
    {
        // This transform will be applied to every proxied request.
        context.AddRequestTransform(transformContext =>
        {
            const string correlationIdHeader = "X-Correlation-ID";
            
            // Attempt to retrieve an existing correlation ID from the incoming request.
            // This allows clients to provide their own ID for end-to-end tracing.
            var correlationId = transformContext.HttpContext.Request.Headers[correlationIdHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                // If no correlation ID is present, generate a new one.
                correlationId = Guid.NewGuid().ToString();
            }

            // Set the header on the downstream request, ensuring it's propagated through the microservices.
            // We use Add, assuming the header doesn't exist. If it could, 'Set' would be safer.
            // Since we check for existence first, 'Add' is appropriate here.
            transformContext.ProxyRequest.Headers.Add(correlationIdHeader, correlationId);
            
            return ValueTask.CompletedTask;
        });
    }
}