using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Infrastructure.Clients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
// Assuming DataServiceClient communicates with a gRPC service defined by REPO-DATA-SERVICE
// For AiModel metadata, DataServiceClient might interact with methods like:
// - GetAiModelMetadata(GetAiModelMetadataRequest) returns AiModelMetadataResponse
// - SaveAiModelMetadata(SaveAiModelMetadataRequest) returns Empty
// For AiModel artifacts:
// - GetAiModelArtifact(GetAiModelArtifactRequest) returns stream or AiModelArtifactResponse with bytes/stream info

namespace AIService.Infrastructure.Persistence
{
    public class ModelRepository : IModelRepository
    {
        private readonly DataServiceClient _dataServiceClient;
        private readonly ILogger<ModelRepository> _logger;

        public ModelRepository(DataServiceClient dataServiceClient, ILogger<ModelRepository> logger)
        {
            _dataServiceClient = dataServiceClient ?? throw new ArgumentNullException(nameof(dataServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AiModel> GetModelAsync(string modelId, string version = null)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                throw new ArgumentNullException(nameof(modelId));

            _logger.LogDebug("Fetching model metadata for ID: {ModelId}, Version: {Version}", modelId, version ?? "latest");
            try
            {
                // DataServiceClient should have a method that maps to the gRPC call in REPO-DATA-SERVICE
                // e.g., _dataServiceClient.GetAiModelMetadataAsync(modelId, version);
                var modelData = await _dataServiceClient.GetAiModelMetadataAsync(modelId, version);

                if (modelData == null)
                {
                    _logger.LogWarning("Model metadata not found for ID: {ModelId}, Version: {Version}", modelId, version ?? "latest");
                    return null;
                }
                
                // Map from DataService's DTO/gRPC message to Domain.AiModel
                // This mapping depends on the structure of modelData from DataServiceClient
                return modelData; // Assuming DataServiceClient already returns the Domain.AiModel
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model metadata for ID: {ModelId}, Version: {Version}", modelId, version ?? "latest");
                throw; // Or handle specific exceptions from DataServiceClient
            }
        }

        public async Task SaveModelAsync(AiModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            _logger.LogDebug("Saving model metadata for ID: {ModelId}, Name: {ModelName}, Version: {ModelVersion}", model.Id, model.Name, model.Version);
            try
            {
                // DataServiceClient should have a method that maps to the gRPC call in REPO-DATA-SERVICE
                // e.g., _dataServiceClient.SaveAiModelMetadataAsync(model);
                await _dataServiceClient.SaveAiModelMetadataAsync(model);
                _logger.LogInformation("Model metadata saved successfully for ID: {ModelId}", model.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving model metadata for ID: {ModelId}", model.Id);
                throw;
            }
        }

        public async Task<Stream> GetModelArtifactStreamAsync(string modelId, string version = null)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                throw new ArgumentNullException(nameof(modelId));

            _logger.LogDebug("Fetching model artifact stream for ID: {ModelId}, Version: {Version}", modelId, version ?? "latest");
            try
            {
                // DataServiceClient should have a method that maps to the gRPC call in REPO-DATA-SERVICE
                // e.g., _dataServiceClient.GetModelArtifactStreamAsync(modelId, version);
                var stream = await _dataServiceClient.GetModelArtifactStreamAsync(modelId, version);

                if (stream == null || stream.Length == 0)
                {
                    _logger.LogWarning("Model artifact stream not found or empty for ID: {ModelId}, Version: {Version}", modelId, version ?? "latest");
                    return null;
                }
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model artifact stream for ID: {ModelId}, Version: {Version}", modelId, version ?? "latest");
                throw;
            }
        }

        public async Task StoreModelArtifactAsync(string modelId, string version, Stream artifactStream, string fileName)
        {
            if (string.IsNullOrWhiteSpace(modelId)) throw new ArgumentNullException(nameof(modelId));
            if (artifactStream == null) throw new ArgumentNullException(nameof(artifactStream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            _logger.LogDebug("Storing model artifact for ID: {ModelId}, Version: {Version}, FileName: {FileName}", modelId, version, fileName);
            try
            {
                await _dataServiceClient.StoreModelArtifactAsync(modelId, version, artifactStream, fileName);
                _logger.LogInformation("Model artifact stored successfully for ID: {ModelId}, Version: {Version}", modelId, version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing model artifact for ID: {ModelId}, Version: {Version}", modelId, version);
                throw;
            }
        }

        public async Task<IEnumerable<AiModel>> ListModelsAsync(string modelType = null, string modelFormat = null)
        {
            _logger.LogDebug("Listing models with Type: {ModelType}, Format: {ModelFormat}", modelType ?? "any", modelFormat ?? "any");
            try
            {
                return await _dataServiceClient.ListAiModelsAsync(modelType, modelFormat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing models from Data Service.");
                throw;
            }
        }
    }
}