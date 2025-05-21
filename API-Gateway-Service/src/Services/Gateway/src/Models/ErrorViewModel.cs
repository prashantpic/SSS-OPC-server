using System.Text.Json.Serialization;

namespace GatewayService.Models
{
    /// <summary>
    /// Defines a standardized error response model for the API Gateway.
    /// This ensures that clients receive error information in a consistent format.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// A unique identifier for the request, useful for tracing and correlation.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        /// <summary>
        /// A human-readable message describing the error.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Optional field for additional error details, such as validation errors or specific error codes.
        /// This could be a string, an object, or an array of error details.
        /// </summary>
        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Details { get; set; }

        /// <summary>
        /// HTTP Status code associated with this error.
        /// </summary>
        [JsonIgnore] // Typically part of the HTTP response, not the body, but can be included if needed.
        public int StatusCode { get; set; }


        public ErrorViewModel(string message)
        {
            Message = message;
        }

        public ErrorViewModel(string requestId, string message, object? details = null)
        {
            RequestId = requestId;
            Message = message;
            Details = details;
        }
    }
}