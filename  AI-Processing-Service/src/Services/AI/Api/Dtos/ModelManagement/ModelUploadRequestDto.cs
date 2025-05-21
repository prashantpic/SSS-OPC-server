using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.ModelManagement
{
    /// <summary>
    /// Data transfer object for carrying AI model file (e.g., ONNX)
    /// and its metadata (name, version, type) for upload via the REST API.
    /// REQ-7-005 (related to adding new models/versions that might later get feedback)
    /// </summary>
    public class ModelUploadRequestDto
    {
        /// <summary>
        /// Name of the AI model.
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ModelName { get; set; }

        /// <summary>
        /// Version of the AI model (e.g., "1.0.0", "2023-03-15").
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string ModelVersion { get; set; }

        /// <summary>
        /// Type of the AI model (e.g., "PredictiveMaintenance", "AnomalyDetection", "NlpIntentClassifier").
        /// Should correspond to Domain.Enums.ModelType.
        /// </summary>
        [Required]
        public string ModelType { get; set; }

        /// <summary>
        /// Format of the AI model file (e.g., "ONNX", "TensorFlowLite", "MLNetZip").
        /// Should correspond to Domain.Enums.ModelFormat.
        /// </summary>
        [Required]
        public string ModelFormat { get; set; }

        /// <summary>
        /// Optional description of the AI model.
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// The AI model file to be uploaded.
        /// </summary>
        [Required]
        public IFormFile ModelFile { get; set; }
    }
}