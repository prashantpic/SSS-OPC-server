using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi
{
    public record EdgeModelMetadataDto(
        string ModelName,
        string Version,
        string FriendlyName, // User-friendly name
        string FilePath,     // Local file path where the model is stored/cached
        DateTime DeploymentTimestampUtc,
        Dictionary<string, string> InputSchema,  // Key: FeatureName, Value: DataType (e.g., "float32", "int64")
        Dictionary<string, string> OutputSchema, // Key: OutputName, Value: DataType
        string? Description = null,
        string? ExecutionProvider = "CPU" // e.g., "CPU", "CUDA", "TensorRT"
    );
}