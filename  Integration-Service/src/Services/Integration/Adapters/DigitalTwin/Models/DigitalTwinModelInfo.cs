using System.Text.Json;

namespace IntegrationService.Adapters.DigitalTwin.Models
{
    /// <summary>
    /// Stores information about digital twin model versions (e.g., DTDL model ID and version)
    /// to assist with compatibility checks and ensure correct data mapping and interaction logic.
    /// REQ-8-011
    /// </summary>
    public record DigitalTwinModelInfo
    {
        /// <summary>
        /// The unique identifier of the Digital Twin model (e.g., DTDL ID like "dtmi:com:example:Thermostat;1").
        /// </summary>
        public required string ModelId { get; init; }

        /// <summary>
        /// The version of the Digital Twin model.
        /// </summary>
        public required string Version { get; init; }

        /// <summary>
        /// The actual definition of the model, typically in JSON format (e.g., DTDL content).
        /// This can be used for validation or dynamic interaction if needed.
        /// Stored as JsonElement for flexibility.
        /// </summary>
        public JsonElement? Definition { get; init; }

        /// <summary>
        /// Display name for the model, if available.
        /// </summary>
        public string? DisplayName { get; init; }
    }
}