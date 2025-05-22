using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi; // Assuming EdgeModelMetadataDto is here

namespace IndustrialAutomation.OpcClient.Domain.Models
{
    /// <summary>
    /// Represents an AI model loaded and ready for execution on the edge client, 
    /// including its configuration and path to the model file.
    /// </summary>
    public class EdgeAiModel
    {
        /// <summary>
        /// Metadata associated with this AI model, including name, version, schema, etc.
        /// </summary>
        public required EdgeModelMetadataDto Metadata { get; set; }

        /// <summary>
        /// The full local file path to the ONNX model file.
        /// </summary>
        public required string FullFilePath { get; set; }

        /// <summary>
        /// Indicates whether the model is currently loaded into the ONNX runtime and ready for inference.
        /// </summary>
        public bool IsLoaded { get; set; } = false;

        /// <summary>
        /// Timestamp of when this model was last loaded or updated.
        /// </summary>
        public System.DateTime? LastLoadTime { get; set; }
    }
}