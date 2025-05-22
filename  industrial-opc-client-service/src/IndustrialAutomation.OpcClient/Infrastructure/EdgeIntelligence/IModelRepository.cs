using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence
{
    public interface IModelRepository
    {
        Task<string> SaveModelAsync(EdgeModelMetadataDto metadata, byte[] modelBytes); // Returns file path
        Task<string?> LoadModelFilePath(string modelName, string version); // Returns file path or null
        Task<EdgeModelMetadataDto?> GetModelMetadata(string modelName, string version); // Returns metadata
        Task<bool> DeleteModelAsync(string modelName, string version);
        Task<List<EdgeModelMetadataDto>> ListModelsAsync();
    }
}