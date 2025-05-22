using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence
{
    public interface IEdgeAiExecutor
    {
        // Returns a unique identifier for the loaded model instance, or the modelName if successful.
        Task<string> LoadModel(string modelFilePath);
        Task<EdgeModelOutputDto> Execute(string modelName, EdgeModelInputDto input);
        Task UnloadModel(string modelName);
        bool IsModelLoaded(string modelName);
    }
}