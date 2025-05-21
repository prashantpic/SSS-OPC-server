using AIService.Domain.Enums;
using System;

namespace AIService.Domain.Models
{
    /// <summary>
    /// Core domain entity representing an AI model, including its metadata (ID, Name, Version, Description),
    /// type (PredictiveMaintenance, AnomalyDetection), format (ONNX, TensorFlowLite),
    /// storage reference (path or URI), input/output schema definition, and current lifecycle status.
    /// Enforces REQ-7-002 and REQ-7-009 through its schema.
    /// </summary>
    public class AiModel
    {
        public string Id { get; private set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public ModelType ModelType { get; set; }
        public ModelFormat ModelFormat { get; set; }
        public string StorageReference { get; set; } // Path, URI, or an identifier for DataService
        public string InputSchema { get; set; } // JSON string defining input structure, names, types, shapes
        public string OutputSchema { get; set; } // JSON string defining output structure, names, types, shapes
        public string Status { get; set; } // e.g., "Registered", "Validated", "Deployed", "Error", "Training"
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }
        public string? Checksum { get; set; } // Checksum of the model artifact

        public AiModel(string id, string name, string version, ModelType modelType, ModelFormat modelFormat, string storageReference, string inputSchema, string outputSchema)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Model ID cannot be null or whitespace.", nameof(id));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Model Name cannot be null or whitespace.", nameof(name));
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Model Version cannot be null or whitespace.", nameof(version));

            Id = id;
            Name = name;
            Version = version;
            ModelType = modelType;
            ModelFormat = modelFormat;
            StorageReference = storageReference ?? throw new ArgumentNullException(nameof(storageReference));
            InputSchema = inputSchema ?? throw new ArgumentNullException(nameof(inputSchema));
            OutputSchema = outputSchema ?? throw new ArgumentNullException(nameof(outputSchema));
            Description = string.Empty;
            Status = "Registered"; // Initial status
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Private constructor for ORM or deserialization
        private AiModel() { }
    }
}