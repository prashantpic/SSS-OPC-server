using System;
using System.Collections.Generic;

namespace IntegrationService.Adapters.IoT.Models
{
    /// <summary>
    /// Data Transfer Object for messages sent to/from IoT platforms.
    /// </summary>
    public record IoTDataMessage
    {
        /// <summary>
        /// Unique identifier for the device originating the data.
        /// </summary>
        public string DeviceId { get; init; } = string.Empty;

        /// <summary>
        /// Timestamp when the data was generated or recorded.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The actual data payload. Can be a structured object or raw data.
        /// Use `object` or a specific base type if payload structure is standardized internally.
        /// </summary>
        public object? Payload { get; init; }

        /// <summary>
        /// Optional metadata associated with the message (e.g., sensor type, location).
        /// </summary>
        public Dictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();

        /// <summary>
        /// Optional identifier for the message, e.g., for tracking.
        /// </summary>
        public string MessageId { get; init; } = Guid.NewGuid().ToString();
    }
}