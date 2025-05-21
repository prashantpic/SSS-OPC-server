using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AIService.Api.Dtos.ModelManagement
{
    public class ModelUploadRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Version { get; set; }

        [Required]
        public string ModelType { get; set; } // e.g., "PredictiveMaintenance", "AnomalyDetection"

        [Required]
        public string ModelFormat { get; set; } // e.g., "ONNX", "TensorFlowLite", "MLNetZip"

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public IFormFile File { get; set; }
    }
}