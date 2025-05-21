using System.Collections.Generic;

namespace IntegrationService.Adapters.IoT.Models
{
    /// <summary>
    /// Data Transfer Object for commands received from IoT platforms.
    /// </summary>
    public record IoTCommand
    {
        /// <summary>
        /// Name of the command to be executed.
        /// </summary>
        public string CommandName { get; init; } = string.Empty;

        /// <summary>
        /// The ID of the device or entity targeted by the command.
        /// </summary>
        public string TargetDeviceId { get; init; } = string.Empty;

        /// <summary>
        /// Parameters for the command. Can be a structured object.
        /// </summary>
        public object? Parameters { get; init; }

        /// <summary>
        /// Optional correlation ID for linking command to response.
        /// </summary>
        public string? CorrelationId { get; init; }
    }
}