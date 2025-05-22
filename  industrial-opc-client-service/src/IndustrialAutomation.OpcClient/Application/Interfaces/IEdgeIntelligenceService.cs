using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Interfaces;

/// <summary>
/// Defines the contract for managing edge AI functionalities, 
/// including model execution, input/output handling, and model updates.
/// </summary>
public interface IEdgeIntelligenceService
{
    Task LoadModelAsync(string modelName, string version);

    Task<EdgeModelOutputDto?> ExecuteModelAsync(string modelName, EdgeModelInputDto input);

    Task HandleModelUpdateAsync(EdgeModelMetadataDto metadata, byte[] modelBytes);

    Task<string?> GetModelStatusAsync(string modelName); // e.g., "Loaded", "Not Found", "Error"
}