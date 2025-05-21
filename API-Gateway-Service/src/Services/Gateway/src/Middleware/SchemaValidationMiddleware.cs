using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
// using NJsonSchema; // Example: For JSON Schema validation - would require adding this package
// using NJsonSchema.Validation; // Example

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

        public SchemaValidationMiddleware(RequestDelegate next, ILogger<SchemaValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Placeholder for determining if validation is needed based on path/method
            bool shouldValidateRequest = ShouldValidateRequest(context.Request);
            bool shouldValidateResponse = ShouldValidateResponse(context.Request); // Response validation often based on request path too

            if (shouldValidateRequest)
            {
                _logger.LogDebug("SchemaValidationMiddleware: Validating request for {Path}", context.Request.Path);
                if (!await ValidateRequestAsync(context))
                {
                    // Validation failed, response already sent by ValidateRequestAsync
                    return;
                }
                _logger.LogDebug("SchemaValidationMiddleware: Request validation passed for {Path}", context.Request.Path);
            }

            if (shouldValidateResponse)
            {
                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context); // Call the next middleware in the pipeline

                    _logger.LogDebug("SchemaValidationMiddleware: Validating response for {Path}", context.Request.Path);
                    if (!await ValidateResponseAsync(context, responseBody))
                    {
                        // If response validation fails, we might log it or, in dev, alter the response.
                        // Altering production responses due to schema mismatch is risky.
                        _logger.LogError("SchemaValidationMiddleware: Response validation failed for {Path}. This might indicate an issue with the downstream service or schema definition.", context.Request.Path);
                        // Potentially, if in dev, return an error
                        // if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                        // {
                        //     context.Response.Clear();
                        //     context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        //     await context.Response.WriteAsync("Response schema validation failed.");
                        //     // Do not copy to originalBodyStream if we clear it
                        //     return; 
                        // }
                    }
                    else
                    {
                        _logger.LogDebug("SchemaValidationMiddleware: Response validation passed for {Path}", context.Request.Path);
                    }
                    
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                await _next(context);
            }
        }

        private bool ShouldValidateRequest(HttpRequest request)
        {
            // Placeholder: Determine if this request needs validation
            // e.g., based on path, method, or presence of a specific header/metadata
            if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put || request.Method == HttpMethods.Patch)
            {
                 // Example: Only validate POSTs to /api/resource
                 // if (request.Path.StartsWithSegments("/api/resource", StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false; // Default to false for placeholder
        }

        private bool ShouldValidateResponse(HttpRequest request) // Response validation decision often depends on the request
        {
            // Placeholder: Determine if the response for this request needs validation
            // if (request.Path.StartsWithSegments("/api/resource", StringComparison.OrdinalIgnoreCase)) return true;
            return false; // Default to false for placeholder
        }


        private async Task<bool> ValidateRequestAsync(HttpContext context)
        {
            if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
            {
                _logger.LogDebug("Request body is empty, skipping validation.");
                return true; // Or handle as invalid if a body is expected
            }

            context.Request.EnableBuffering(); // Allow the request body to be read multiple times

            string requestBody;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset the stream position for the next middleware
            }

            _logger.LogTrace("Request body for validation: {RequestBody}", requestBody);

            // --- Placeholder for actual schema validation logic ---
            // 1. Load the appropriate schema (e.g., from a file, cache, or schema registry based on context.Request.Path)
            //    var schemaJson = await LoadSchemaAsync(context.Request.Path, "request");
            //    if (schemaJson == null) { _logger.LogWarning("No request schema found for {Path}", context.Request.Path); return true; /* or false */ }
            // 2. Use a validation library (e.g., NJsonSchema, Json.NET Schema)
            //    try 
            //    {
            //        var schema = await JsonSchema.FromJsonAsync(schemaJson);
            //        var validator = new JsonSchemaValidator();
            //        var validationErrors = validator.Validate(requestBody, schema);
            //        if (validationErrors.Any())
            //        {
            //            _logger.LogWarning("Request schema validation failed for {Path}: {Errors}", context.Request.Path, string.Join(", ", validationErrors.Select(e => e.Path + ": " + e.Kind)));
            //            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            //            await context.Response.WriteAsJsonAsync(new { errors = validationErrors.Select(e => e.ToString()).ToList() });
            //            return false;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error during request schema validation for {Path}", context.Request.Path);
            //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //        await context.Response.WriteAsync("Error during request schema validation.");
            //        return false;
            //    }
            _logger.LogWarning("SchemaValidationMiddleware: Request validation logic is a placeholder. Path: {Path}", context.Request.Path);
            // --- End Placeholder ---

            return true; // Assume validation passes for placeholder
        }

        private async Task<bool> ValidateResponseAsync(HttpContext context, MemoryStream responseBodyBuffer)
        {
            responseBodyBuffer.Seek(0, SeekOrigin.Begin);
            string responseBody = await new StreamReader(responseBodyBuffer).ReadToEndAsync();
            responseBodyBuffer.Seek(0, SeekOrigin.Begin); // Reset for copying to original stream

            if (string.IsNullOrWhiteSpace(responseBody))
            {
                _logger.LogDebug("Response body is empty, skipping validation.");
                return true;
            }
            _logger.LogTrace("Response body for validation: {ResponseBody}", responseBody);

            // --- Placeholder for actual schema validation logic ---
            // 1. Load the appropriate schema (e.g., from a file, cache, or schema registry based on context.Request.Path and response status code)
            //    var schemaJson = await LoadSchemaAsync(context.Request.Path, "response", context.Response.StatusCode);
            //    if (schemaJson == null) { _logger.LogWarning("No response schema found for {Path}", context.Request.Path); return true; /* or false */ }
            // 2. Use a validation library
            //    try
            //    {
            //        var schema = await JsonSchema.FromJsonAsync(schemaJson);
            //        var validator = new JsonSchemaValidator();
            //        var validationErrors = validator.Validate(responseBody, schema);
            //        if (validationErrors.Any())
            //        {
            //            _logger.LogError("Response schema validation failed for {Path}: {Errors}", context.Request.Path, string.Join(", ", validationErrors.Select(e => e.Path + ": " + e.Kind)));
            //            return false; // Logged, but original response will likely still be sent unless in strict dev mode
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _logger.LogError(ex, "Error during response schema validation for {Path}", context.Request.Path);
            //        return false;
            //    }
            _logger.LogWarning("SchemaValidationMiddleware: Response validation logic is a placeholder. Path: {Path}", context.Request.Path);
            // --- End Placeholder ---

            return true; // Assume validation passes for placeholder
        }

        // Placeholder for schema loading logic
        // private async Task<string?> LoadSchemaAsync(PathString path, string type, int? statusCode = null)
        // {
        //    // Logic to find and load schema file based on path, type (request/response), and statusCode
        //    // E.g., map /api/users to schemas/users_request.json or schemas/users_response_200.json
        //    return null; 
        // }
    }
}