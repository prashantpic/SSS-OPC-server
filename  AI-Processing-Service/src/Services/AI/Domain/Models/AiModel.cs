namespace AIService.Domain.Models
{
    using AIService.Domain.Enums;

    /// <summary>
    /// Core domain entity representing an AI model.
    /// Includes metadata, type, format, storage reference, schema, and status.
    /// Enforces REQ-7-002 and REQ-7-009 through its schema definitions.
    /// </summary>
    public class AiModel
    {
        /// <summary>
        /// Unique identifier for the AI model.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Human-readable name of the AI model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Version of the AI model.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Description of the AI model.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of the AI model (e.g., PredictiveMaintenance, AnomalyDetection).
        /// </summary>
        public ModelType ModelType { get; set; }

        /// <summary>
        /// File format of the AI model (e.g., ONNX, TensorFlowLite).
        /// </summary>
        public ModelFormat ModelFormat { get; set; }

        /// <summary>
        /// Reference to the storage location of the model artifact (e.g., path, URI, or Data Service ID).
        /// </summary>
        public string StorageReference { get; set; }

        /// <summary>
        /// JSON string or custom object defining the expected input schema (features, names, types, shapes).
        /// Used for ModelInput validation (REQ-7-002, REQ-7-009).
        /// </summary>
        public string InputSchema { get; set; } // Could be a more structured object or validated JSON

        /// <summary>
        /// JSON string or custom object defining the expected output schema (features, names, types, shapes).
        /// Used for parsing ModelOutput.
        /// </summary>
        public string OutputSchema { get; set; } // Could be a more structured object

        /// <summary>
        /// Current lifecycle status of the model (e.g., Registered, Deployed, Retired).
        /// </summary>
        public string Status { get; set; } // Could be an enum
    }
}