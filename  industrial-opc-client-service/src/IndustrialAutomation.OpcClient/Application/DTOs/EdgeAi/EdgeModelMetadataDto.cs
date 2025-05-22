using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi
{
    public record EdgeModelMetadataDto
    {
        public string ModelName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? FilePath { get; set; } // Path where the model is stored locally
        public DateTime DeploymentTimestamp { get; set; } = DateTime.UtcNow; // When this metadata was created/deployed
        public Dictionary<string, string>? InputSchema { get; set; } // Key: Feature Name, Value: Data Type (e.g., "float32", "int64")
        public Dictionary<string, string>? OutputSchema { get; set; } // Key: Output Name, Value: Data Type
        public string? Description { get; set; }
        public string? Framework { get; set; } // e.g., "ONNX", "TensorFlowLite"
        public string? Checksum { get; set; } // Checksum of the model file for integrity
        public string? SourceUrl { get; set; } // URL from where the model was downloaded
    }

    public record EdgeModelStatusDto : EdgeModelMetadataDto
    {
        public bool IsLoaded { get; init; }
        public DateTime? LastLoadAttempt { get; init; }
        public string? LastLoadError { get; init; }
    }

    // Used by gRPC client if model bytes are transferred
    public record ModelUpdateRequestDto
    {
        public EdgeModelMetadataDto Metadata { get; init; } = new();
        public byte[] ModelBytes { get; init; } = Array.Empty<byte>();
    }
}