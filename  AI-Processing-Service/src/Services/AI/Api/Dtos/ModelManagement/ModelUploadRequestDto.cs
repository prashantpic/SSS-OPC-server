using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.ModelManagement
{
    public class ModelUploadRequestDto
    {
        /// <summary>
        /// Human-readable name for the AI model.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Version string for the AI model (e.g., "1.0.0", "2023-04-01").
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Version { get; set; }

        /// <summary>
        /// Brief description of the AI model's purpose or capabilities.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Type of the model, e.g., "PredictiveMaintenance", "AnomalyDetection", "NlpIntentClassifier".
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ModelType { get; set; }

        /// <summary>
        /// Format of the model file, e.g., "ONNX", "TensorFlowLite", "MLNetZip".
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ModelFormat { get; set; }

        /// <summary>
        /// The AI model file being uploaded (e.g., .onnx, .zip).
        /// REQ-7-005 (implicitly, model management covers upload), REQ-DLP-024
        /// </summary>
        [Required]
        public IFormFile ModelFile { get; set; }

        /// <summary>
        /// Optional: JSON string defining the expected input schema for the model.
        /// Used for validation and understanding model inputs.
        /// REQ-7-002, REQ-7-009
        /// </summary>
        public string InputSchema { get; set; }

        /// <summary>
        /// Optional: JSON string defining the expected output schema for the model.
        /// Used for parsing model outputs.
        /// </summary>
        public string OutputSchema { get; set; }
    }
}