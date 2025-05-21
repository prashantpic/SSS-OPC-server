using System.Text.Json;

namespace IntegrationService.Adapters.IoT.Models
{
    /// <summary>
    /// Represents a command structure received from an IoT platform, intended for bi-directional communication.
    /// REQ-8-005
    /// </summary>
    public record IoTCommand
    {
        /// <summary>
        /// Name of the command to be executed.
        /// </summary>
        public required string CommandName { get; init; }

        /// <summary>
        /// Identifier of the target device for the command.
        /// </summary>
        public required string TargetDeviceId { get; init; }

        /// <summary>
        /// Parameters for the command.
        /// Using JsonElement to represent a flexible JSON structure for parameters.
        /// </summary>
        public JsonElement? Parameters { get; init; }

        /// <summary>
        /// Optional correlation ID to track command execution and response.
        /// </summary>
        public string? CorrelationId { get; init; }
    }

    /// <summary>
    /// Represents a response to an IoT command.
    /// </summary>
    public record IoTCommandResponse
    {
        /// <summary>
        /// Identifier of the device that processed the command.
        /// </summary>
        public required string DeviceId { get; init; }

        /// <summary>
        /// Correlation ID linking this response to the original command.
        /// </summary>
        public required string CorrelationId { get; init; }

        /// <summary>
        /// Status of the command execution (e.g., 200 for success, 500 for error).
        /// </summary>
        public int Status { get; init; }

        /// <summary>
        /// Payload of the response, if any.
        /// Using JsonElement to represent a flexible JSON structure.
        /// </summary>
        public JsonElement? Payload { get; init; }
    }
}