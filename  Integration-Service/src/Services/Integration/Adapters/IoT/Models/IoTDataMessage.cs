using System;
using System.Collections.Generic;
using System.Text.Json;

namespace IntegrationService.Adapters.IoT.Models
{
    /// <summary>
    /// Represents a generic data payload for telemetry or events being sent to or received from an IoT platform.
    /// REQ-8-005
    /// </summary>
    public record IoTDataMessage
    {
        /// <summary>
        /// Identifier of the device sending or targeted by the message.
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Timestamp of when the data was generated or received.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// The actual data payload. Can be a complex object.
        /// Using JsonElement to represent a flexible JSON structure.
        /// </summary>
        public JsonElement Payload { get; init; }

        /// <summary>
        /// Optional metadata associated with the message.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Metadata { get; init; }
    }
}