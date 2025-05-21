```csharp
using AIService.Domain.Interfaces;
using AIService.Domain.Models; // For AiModel
using AIService.Infrastructure.Clients; // For DataServiceClient
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

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
            {
                throw new ArgumentException("Model ID cannot be null or whitespace.", nameof(modelId));
            }

            _logger.LogDebug("Fetching model metadata for ID: {ModelId}, Version: {Version} from DataService.", modelId, version ?? "latest");
            try
            {
                // Assuming DataServiceClient has a method to get AiModel metadata
                var model = await _dataServiceClient.GetAiModelMetadataAsync(modelId, version);
                if (model == null)
                {
                    _logger.LogWarning("Model metadata for ID: {ModelId}, Version: {Version} not found in DataService.", modelId, version ?? "latest");
                }
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model metadata for ID: {ModelId}, Version: {Version} from DataService.", modelId, version ?? "latest");
                throw; // Or handle more gracefully, e.g., return null or throw custom exception
            }
        }

        public async Task<Stream> GetModelArtifactStreamAsync(string modelId, string version = null)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                throw new ArgumentException("Model ID cannot be null or whitespace.", nameof(modelId));
            }
            _logger.LogDebug("Fetching model artifact stream for ID: {ModelId}, Version: {Version} from DataService.", modelId, version ?? "latest");
            try
            {
                // Assuming DataServiceClient has a method to get the artifact stream
                var stream = await _dataServiceClient.GetModelArtifactStreamAsync(modelId, version);
                if (stream == null || stream == Stream.Null || (stream.CanSeek && stream.Length == 0))
                {
                     _logger.LogWarning("Model artifact stream for ID: {ModelId}, Version: {Version} not found or is empty in DataService.", modelId, version ?? "latest");
                    //  throw new FileNotFoundException($"Model artifact for ID {modelId}, Version {version ?? "latest"} not found or is empty.");
                    return null; // Or throw
                }
                return stream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching model artifact stream for ID: {ModelId}, Version: {Version} from DataService.", modelId, version ?? "latest");
                throw;
            }
        }

        public async Task<AiModel> SaveModelAsync(AiModel model, Stream modelArtifactStream = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
             if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Version))
            {
                throw new ArgumentException("Model Name and Version must be provided.", nameof(model));
            }

            _logger.LogDebug("Saving model metadata for Name: {ModelName}, Version: {ModelVersion} to DataService.", model.Name, model.Version);
            try
            {
                // This might be a two-step process if DataServiceClient separates metadata and artifact storage.
                // 1. Store artifact if stream is provided, get back a storage reference.
                // 2. Update model.StorageReference with this new reference.
                // 3. Save AiModel metadata.

                if (modelArtifactStream != null)
                {
                    _logger.LogDebug("Uploading model artifact for {ModelName} version {ModelVersion}.", model.Name, model.Version);
                    // Assuming DataServiceClient has a method to store an artifact and it returns a reference
                    // This method might take model.Id and model.Version as part of the key.
                    string storageReference = await _dataServiceClient.SaveModelArtifactAsync(model, modelArtifactStream);
                    if (string.IsNullOrWhiteSpace(storageReference))
                    {
                        _logger.LogError("Failed to save model artifact for {ModelName} version {ModelVersion}; storage reference was empty.", model.Name, model.Version);
                        throw new Exception("Failed to save model artifact; no storage reference returned.");
                    }
                    model.StorageReference = storageReference; // Update the model with the actual storage path/ID
                     _logger.LogInformation("Model artifact for {ModelName} version {ModelVersion} saved. StorageReference: {StorageReference}", model.Name, model.Version, storageReference);
                }
                else if (string.IsNullOrWhiteSpace(model.StorageReference))
                {
                    _logger.LogWarning("Saving model {ModelName} version {ModelVersion} without an artifact stream or an existing storage reference.", model.Name, model.Version);
                }
                
                // Assuming DataServiceClient has a method to save AiModel metadata
                var savedModel = await _dataServiceClient.SaveAiModelMetadataAsync(model);
                _logger.LogInformation("Model metadata for Name: {ModelName}, Version: {ModelVersion} saved successfully to DataService. ID: {ModelId}", savedModel.Name, savedModel.Version, savedModel.Id);
                return savedModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving model metadata for Name: {ModelName}, Version: {ModelVersion} to DataService.", model.Name, model.Version);
                throw;
            }
        }

        public async Task DeleteModelAsync(string modelId, string version = null)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                throw new ArgumentException("Model ID cannot be null or whitespace.", nameof(modelId));
            }
            _logger.LogInformation("Requesting deletion of model ID: {ModelId}, Version: {Version} from DataService.", modelId, version ?? "all");
            try
            {
                await _dataServiceClient.DeleteAiModelAsync(modelId, version);
                _logger.LogInformation("Model ID: {ModelId}, Version: {Version} deletion request sent to DataService.", modelId, version ?? "all");
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error deleting model ID: {ModelId}, Version: {Version} from DataService.", modelId, version ?? "all");
                throw;
            }
        }
    }
}