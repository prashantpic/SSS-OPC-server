using System.Net;
using System.Text.Json;
using ManagementService.Domain.SeedWork; // For DomainException, EntityNotFoundException
using FluentValidation; // For ValidationException

namespace ManagementService.Api.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {ErrorMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponseModel();

        switch (exception)
        {
            case ValidationException validationException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Validation failed.";
                errorResponse.Errors = validationException.Errors.Select(e => new ErrorDetail { PropertyName = e.PropertyName, Message = e.ErrorMessage }).ToList();
                break;
            case EntityNotFoundException entityNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = entityNotFoundException.Message;
                break;
            case DomainException domainException:
                response.StatusCode = (int)HttpStatusCode.BadRequest; // Or UnprocessableEntity depending on context
                errorResponse.Message = domainException.Message;
                break;
            // Add other custom exception types here
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = _env.IsDevelopment() ? exception.Message : "An unexpected internal server error occurred.";
                if (_env.IsDevelopment())
                {
                    errorResponse.StackTrace = exception.StackTrace;
                }
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
        return response.WriteAsync(result);
    }
}

public class ErrorResponseModel
{
    public string Message { get; set; } = string.Empty;
    public List<ErrorDetail>? Errors { get; set; }
    public string? StackTrace { get; set; }
}

public class ErrorDetail
{
    public string PropertyName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}