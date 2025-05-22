using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence
{
    public interface IModelRepository
    {
        // Saves the model bytes and its metadata. Returns the path where the model file was saved.
        Task<string> SaveModelAsync(EdgeModelMetadataDto metadata, byte[] modelBytes);

        // Gets the file path for a given model name and version.
        Task<string?> GetModelFilePathAsync(string modelName, string version);

        // Gets the metadata for a given model name and version.
        Task<EdgeModelMetadataDto?> GetModelMetadata(string modelName, string version);

        // Lists all available models' metadata.
        Task<IEnumerable<EdgeModelMetadataDto>> ListModelsAsync();

        // Deletes a specific model version.
        Task<bool> DeleteModelAsync(string modelName, string version);
    }
}