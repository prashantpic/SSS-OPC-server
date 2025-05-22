using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence
{
    public interface IEdgeAiExecutor
    {
        // Loads a model and prepares it for execution.
        // Model name and version are used to uniquely identify the loaded session.
        Task LoadModel(string modelFilePath, string modelName, string version);

        // Executes a pre-loaded model.
        Task<EdgeModelOutputDto?> Execute(string modelName, string version, EdgeModelInputDto input);

        // Optional: Unload a model to free resources
        // Task UnloadModel(string modelName, string version);

        // Optional: Check if a specific model version is loaded
        // bool IsModelLoaded(string modelName, string version);
    }
}