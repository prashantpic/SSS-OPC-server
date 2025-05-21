namespace IntegrationService.Adapters.DigitalTwin.Models
{
    /// <summary>
    /// Model for sending updates to a Digital Twin.
    /// The structure should be specific to the target Digital Twin platform's API.
    /// </summary>
    public record DigitalTwinUpdateRequest
    {
        /// <summary>
        /// The payload for the update. The structure depends on the target platform (e.g., JSON Patch, key-value pairs).
        /// Use `object` to allow flexibility.
        /// </summary>
        public object UpdatePayload { get; init; } = new object();

        /// <summary>
        /// Indicates the type of update (e.g., PropertyUpdate, Telemetry, Command).
        /// </summary>
        public string UpdateType { get; init; } = string.Empty;

        /// <summary>
        /// Optional correlation ID for tracking the update.
        /// </summary>
        public string? CorrelationId { get; init; }
    }
}