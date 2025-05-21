using System;
using System.Collections.Generic;

namespace GatewayService.Models
{
    /// <summary>
    /// Defines a standardized error response model for the API Gateway.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// A unique identifier for the request, useful for tracing.
        /// </summary>
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// A human-readable message explaining the error.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional application-specific error code.
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The path of the request that resulted in an error.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// A dictionary of validation errors, where the key is the field name and the value is an array of error messages.
        /// </summary>
        public Dictionary<string, string[]>? Errors { get; set; }

        public ErrorViewModel()
        {
            Timestamp = DateTime.UtcNow;
        }

        public ErrorViewModel(int statusCode, string message)
        : this()
        {
            StatusCode = statusCode;
            Message = message;
        }

        public ErrorViewModel(int statusCode, string message, string requestId, string? path = null)
        : this(statusCode, message)
        {
            RequestId = requestId;
            Path = path;
        }
    }
}