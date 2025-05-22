using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi
{
    public record EdgeModelInputDto(
        string ModelName,
        string? ModelVersion, // Optional, if specific version needed for execution context
        Dictionary<string, object> Features // Key: Feature name expected by the model, Value: Feature value
    );
}