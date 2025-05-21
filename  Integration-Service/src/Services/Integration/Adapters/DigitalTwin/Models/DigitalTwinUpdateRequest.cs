using System.Collections.Generic;
using System.Text.Json;

namespace IntegrationService.Adapters.DigitalTwin.Models
{
    /// <summary>
    /// Defines the payload for updating properties on a digital twin using JSON Patch format.
    /// REQ-8-010, REQ-8-011
    /// </summary>
    public record DigitalTwinUpdateRequest
    {
        /// <summary>
        /// Identifier of the Digital Twin to update.
        /// </summary>
        public required string TwinId { get; init; }

        /// <summary>
        /// A list of JSON Patch operations to apply to the Digital Twin.
        /// </summary>
        public required IReadOnlyList<JsonPatchOperation> PatchOperations { get; init; }
    }

    /// <summary>
    /// Represents a single JSON Patch operation.
    /// See RFC 6902 for JSON Patch specification.
    /// </summary>
    public record JsonPatchOperation
    {
        /// <summary>
        /// The operation to perform (e.g., "add", "remove", "replace", "move", "copy", "test").
        /// </summary>
        public required string Op { get; init; }

        /// <summary>
        /// The JSON Pointer path to the target location.
        /// </summary>
        public required string Path { get; init; }

        /// <summary>
        /// The value to be used for "add", "replace", and "test" operations.
        /// For "move" and "copy", this specifies the source path.
        /// Not used for "remove".
        /// </summary>
        public JsonElement? Value { get; init; }

        /// <summary>
        /// For "move" or "copy" operations, this is the JSON Pointer path to the source location.
        /// </summary>
        public string? From { get; init; }
    }
}