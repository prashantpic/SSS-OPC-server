using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// using NJsonSchema; // Example schema validation library

namespace GatewayService.Middleware
{
    /// <summary>
    /// Validates request/response payloads against defined schemas.
    /// ASP.NET Core middleware that validates incoming request payloads and/or outgoing response payloads
    /// against defined schemas (e.g., JSON Schemas derived from OpenAPI specifications).
    /// Ensures data integrity and contract adherence.
    /// </summary>
    public class SchemaValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SchemaValidationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _isEnabled;

        public SchemaValidationMiddleware(
            RequestDelegate next,
            ILogger<SchemaValidationMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _isEnabled = _configuration.GetSection("FeatureFlags").Get<string[]>()?.Contains("enableSchemaValidation") ?? false;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_isEnabled)
            {
                await _next(context);
                return;
            }

            _logger.LogDebug("SchemaValidationMiddleware: Processing request {Path}", context.Request.Path);

            // TODO: Implement schema loading/registry logic. Schemas might be loaded from files, a database, or configuration.
            // For this example, we'll assume a placeholder for schema retrieval.

            // 1. Request Validation (if applicable for the route/method)
            if (context.Request.ContentLength > 0 && (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put || context.Request.Method == HttpMethods.Patch))
            {
                context.Request.EnableBuffering(); // Allow request body to be read multiple times

                string requestBody;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset stream position
                }

                // Placeholder for schema fetching based on context.Request.Path and context.Request.Method
                // var requestSchemaJson = GetRequestSchemaForPath(context.Request.Path, context.Request.Method);
                string requestSchemaJson = null; // Replace with actual schema retrieval

                if (!string.IsNullOrEmpty(requestSchemaJson))
                {
                    _logger.LogDebug("Validating request body for {Path} against schema.", context.Request.Path);
                    // var schema = await JsonSchema.FromJsonAsync(requestSchemaJson);
                    // var validationErrors = schema.Validate(requestBody);
                    // if (validationErrors.Any())
                    // {
                    //     _logger.LogWarning("Request schema validation failed for {Path}: {Errors}", context.Request.Path, string.Join(", ", validationErrors.Select(e => e.Path + ": " + e.Kind)));
                    //     context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    //     await context.Response.WriteAsync($"Invalid request payload: {string.Join(", ", validationErrors.Select(e => e.Message))}");
                    //     return;
                    // }
                    _logger.LogDebug("Request body validation successful for {Path}.", context.Request.Path);
                }
            }

            // 2. Buffer response for validation (if response validation is enabled)
            Stream originalResponseBodyStream = null;
            MemoryStream responseBodyStream = null;
            // bool shouldValidateResponse = ShouldValidateResponse(context.Request.Path, context.Request.Method); // Placeholder
            bool shouldValidateResponse = false; 

            if (shouldValidateResponse)
            {
                originalResponseBodyStream = context.Response.Body;
                responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;
            }

            await _next(context);

            // 3. Response Validation (if applicable and response buffered)
            if (shouldValidateResponse && responseBodyStream != null)
            {
                context.Response.Body.Seek(0, SeekOrigin.Begin);
                string responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin); // Reset stream for client

                // Placeholder for schema fetching
                // var responseSchemaJson = GetResponseSchemaForPath(context.Request.Path, context.Request.Method, context.Response.StatusCode);
                string responseSchemaJson = null; // Replace with actual schema retrieval

                if (!string.IsNullOrEmpty(responseSchemaJson))
                {
                     _logger.LogDebug("Validating response body for {Path} against schema.", context.Request.Path);
                    // var schema = await JsonSchema.FromJsonAsync(responseSchemaJson);
                    // var validationErrors = schema.Validate(responseBody);
                    // if (validationErrors.Any())
                    // {
                    //     _logger.LogError("Response schema validation failed for {Path}: {Errors}. This indicates an issue with the downstream service or gateway transformation.",
                    //         context.Request.Path, string.Join(", ", validationErrors.Select(e => e.Path + ": " + e.Kind)));
                    //     // Decide on action: log, alter response to generic error, etc.
                    //     // For now, just log. Client will receive the original (invalid) response from downstream.
                    // } else {
                    //     _logger.LogDebug("Response body validation successful for {Path}.", context.Request.Path);
                    // }
                }
                await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                context.Response.Body = originalResponseBodyStream; // Restore original stream
                responseBodyStream.Dispose();
            }
        }

        // Placeholder methods for schema retrieval logic
        // private string GetRequestSchemaForPath(PathString path, string method) { /* TODO */ return null; }
        // private string GetResponseSchemaForPath(PathString path, string method, int statusCode) { /* TODO */ return null; }
        // private bool ShouldValidateResponse(PathString path, string method) { /* TODO */ return false; }
    }
}