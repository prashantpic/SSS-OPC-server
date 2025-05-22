using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi
{
    public record EdgeModelOutputDto(
        string ModelName,
        string ModelVersion,
        DateTime InferenceTimestampUtc,
        Dictionary<string, object> Results, // Key: Output name from the model, Value: Output value
        string Status = "Success", // e.g., "Success", "Error", "ThresholdExceeded"
        Dictionary<string, object>? Metadata = null // Any relevant output metadata from processing
    );
}