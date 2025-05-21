namespace IntegrationService.Adapters.DigitalTwin.Models
{
    /// <summary>
    /// Model representing versioning and compatibility information for a Digital Twin model.
    /// </summary>
    public record DigitalTwinModelInfo
    {
        /// <summary>
        /// The unique identifier for the Digital Twin model (e.g., DTDL model ID).
        /// </summary>
        public string ModelId { get; init; } = string.Empty;

        /// <summary>
        /// The version of the Digital Twin model.
        /// </summary>
        public string Version { get; init; } = string.Empty;

        /// <summary>
        /// Any additional capability or schema information needed for compatibility checks.
        /// </summary>
        public object? Schema { get; init; } // Could be the full DTDL JSON, or a parsed structure
    }
}