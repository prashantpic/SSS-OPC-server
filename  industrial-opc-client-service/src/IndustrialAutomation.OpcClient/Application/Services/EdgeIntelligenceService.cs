using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence;
using IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc;
using IndustrialAutomation.OpcClient.Infrastructure.DataHandling;
using IndustrialAutomation.OpcClient.Domain.Models; // For BufferedDataItem
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class EdgeIntelligenceService : IEdgeIntelligenceService
    {
        private readonly ILogger<EdgeIntelligenceService> _logger;
        private readonly IEdgeAiExecutor _aiExecutor;
        private readonly IModelRepository _modelRepository;
        private readonly IServerAppGrpcClient? _grpcClient; // For model updates from server
        private readonly IDataBufferer _dataBufferer; // To buffer AI outputs if server comms fail
        private readonly IDataTransmissionService _dataTransmissionService; // To send AI outputs

        private ConcurrentDictionary<string, Domain.Models.EdgeAiModel> _loadedModels; // Domain model

        public EdgeIntelligenceService(
            ILogger<EdgeIntelligenceService> logger,
            IEdgeAiExecutor aiExecutor,
            IModelRepository modelRepository,
            IDataBufferer dataBufferer,
            IDataTransmissionService dataTransmissionService,
            // IServerAppGrpcClient? grpcClient = null // Injected if available
            IServiceProvider serviceProvider
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _aiExecutor = aiExecutor ?? throw new ArgumentNullException(nameof(aiExecutor));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _dataBufferer = dataBufferer ?? throw new ArgumentNullException(nameof(dataBufferer));
            _dataTransmissionService = dataTransmissionService ?? throw new ArgumentNullException(nameof(dataTransmissionService));
            // _grpcClient = grpcClient;
            _grpcClient = serviceProvider.GetService(typeof(IServerAppGrpcClient)) as IServerAppGrpcClient;

            _loadedModels = new ConcurrentDictionary<string, Domain.Models.EdgeAiModel>();
        }

        public async Task<bool> LoadModelAsync(string modelName, string version)
        {
            if (string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(version))
            {
                _logger.LogError("Model name or version cannot be empty.");
                return false;
            }

            string modelKey = $"{modelName}:{version}";
            if (_loadedModels.ContainsKey(modelKey) && _loadedModels[modelKey].IsLoaded)
            {
                _logger.LogInformation("Model {ModelKey} is already loaded.", modelKey);
                return true;
            }

            try
            {
                var modelMetadata = await _modelRepository.GetModelMetadata(modelName, version);
                if (modelMetadata == null || string.IsNullOrEmpty(modelMetadata.FilePath))
                {
                    _logger.LogError("Metadata or file path not found for model {ModelKey}.", modelKey);
                    // Attempt to download if gRPC client available
                    if (_grpcClient != null) {
                        _logger.LogInformation("Attempting to download model {ModelKey} from server.", modelKey);
                        // This is a conceptual call, actual proto might differ
                        var modelUpdate = await _grpcClient.GetModelAsync(modelName, version); 
                        if (modelUpdate != null && modelUpdate.ModelBytes != null && modelUpdate.Metadata != null) {
                            await HandleModelUpdateAsync(modelUpdate.Metadata, modelUpdate.ModelBytes);
                            modelMetadata = await _modelRepository.GetModelMetadata(modelName, version); // try again
                            if (modelMetadata == null || string.IsNullOrEmpty(modelMetadata.FilePath))
                            {
                                 _logger.LogError("Failed to load model {ModelKey} even after download attempt.", modelKey);
                                 return false;
                            }
                        } else {
                            _logger.LogError("Failed to download model {ModelKey} from server.", modelKey);
                            return false;
                        }
                    } else {
                        return false;
                    }
                }

                string modelFilePath = Path.Combine(modelMetadata.FilePath); // Assuming FilePath is already full or relative to a known base

                if (!File.Exists(modelFilePath))
                {
                    _logger.LogError("Model file does not exist at path: {ModelFilePath} for model {ModelKey}", modelFilePath, modelKey);
                    return false;
                }

                await _aiExecutor.LoadModel(modelFilePath, modelName, version); // Pass name and version for executor to manage

                var domainModel = new Domain.Models.EdgeAiModel
                {
                    Metadata = modelMetadata,
                    FullFilePath = modelFilePath,
                    IsLoaded = true
                };
                _loadedModels.AddOrUpdate(modelKey, domainModel, (k, o) => domainModel);

                _logger.LogInformation("Successfully loaded AI model {ModelKey} from {ModelFilePath}", modelKey, modelFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load AI model {ModelKey}", modelKey);
                var domainModel = new Domain.Models.EdgeAiModel { Metadata = new EdgeModelMetadataDto { ModelName = modelName, Version = version}, IsLoaded = false };
                _loadedModels.AddOrUpdate(modelKey, domainModel, (k, o) => domainModel);
                return false;
            }
        }

        public async Task<EdgeModelOutputDto?> ExecuteModelAsync(string modelName, string version, EdgeModelInputDto input)
        {
            if (string.IsNullOrEmpty(modelName) || string.IsNullOrEmpty(version))
            {
                _logger.LogError("Model name or version cannot be empty for execution.");
                return null;
            }
             if (input == null)
            {
                _logger.LogError("Input DTO for model {ModelName}:{Version} cannot be null.", modelName, version);
                return null;
            }

            input.ModelName = modelName; // Ensure input DTO has model context if executor needs it
            input.ModelVersion = version;

            string modelKey = $"{modelName}:{version}";
            if (!_loadedModels.TryGetValue(modelKey, out var modelInfo) || !modelInfo.IsLoaded)
            {
                _logger.LogWarning("Attempting to execute model {ModelKey} which is not loaded. Trying to load now...", modelKey);
                bool loaded = await LoadModelAsync(modelName, version);
                if (!loaded || !_loadedModels.TryGetValue(modelKey, out modelInfo) || !modelInfo.IsLoaded)
                {
                     _logger.LogError("Failed to load model {ModelKey} for execution.", modelKey);
                    return null;
                }
            }

            try
            {
                _logger.LogDebug("Executing AI model {ModelKey} with input features: {FeatureKeys}", modelKey, string.Join(", ", input.Features.Keys));
                var output = await _aiExecutor.Execute(modelName, version, input); // Executor uses name/version to find loaded session

                if (output != null)
                {
                    _logger.LogInformation("Successfully executed AI model {ModelKey}. Output generated.", modelKey);
                    // Optionally send output to server
                    await _dataTransmissionService.SendEdgeAiOutputAsync(output);
                }
                else
                {
                    _logger.LogWarning("AI model {ModelKey} execution returned null output.", modelKey);
                }
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing AI model {ModelKey}", modelKey);
                return new EdgeModelOutputDto { ModelName = modelName, ModelVersion = version, InferenceTimestamp = DateTime.UtcNow, Results = new Dictionary<string, object>(), Status = $"Error: {ex.Message}"};
            }
        }

        public async Task HandleModelUpdateAsync(EdgeModelMetadataDto metadata, byte[] modelBytes)
        {
            if (metadata == null || string.IsNullOrEmpty(metadata.ModelName) || string.IsNullOrEmpty(metadata.Version))
            {
                _logger.LogError("Invalid metadata provided for model update.");
                return;
            }
            if (modelBytes == null || modelBytes.Length == 0)
            {
                _logger.LogError("Model file bytes are null or empty for model {ModelName}:{Version}.", metadata.ModelName, metadata.Version);
                return;
            }

            _logger.LogInformation("Handling model update for {ModelName}:{Version}.", metadata.ModelName, metadata.Version);
            try
            {
                // Save the new model using IModelRepository
                string savedPath = await _modelRepository.SaveModelAsync(metadata, modelBytes);
                metadata.FilePath = savedPath; // Update metadata with the actual saved path

                _logger.LogInformation("New model version {ModelName}:{Version} saved to {FilePath}.", metadata.ModelName, metadata.Version, savedPath);

                // Unload existing model if loaded
                string modelKey = $"{metadata.ModelName}:{metadata.Version}";
                if (_loadedModels.TryRemove(modelKey, out var oldModel) && oldModel.IsLoaded)
                {
                    //_aiExecutor.UnloadModel(oldModel.Metadata.ModelName, oldModel.Metadata.Version); // if executor supports unload
                    _logger.LogInformation("Unloaded previous version of model {ModelKey} if it was active.", modelKey);
                }
                
                // The model will be loaded on its next execution request, or explicitly if needed
                _logger.LogInformation("Model {ModelKey} updated. It will be loaded on next use or by explicit call.", modelKey);
                // Optionally, pre-load it: await LoadModelAsync(metadata.ModelName, metadata.Version);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle model update for {ModelName}:{Version}", metadata.ModelName, metadata.Version);
            }
        }

        public Task<EdgeModelStatusDto?> GetModelStatusAsync(string modelName, string version)
        {
            string modelKey = $"{modelName}:{version}";
            if (_loadedModels.TryGetValue(modelKey, out var modelInfo))
            {
                return Task.FromResult<EdgeModelStatusDto?>(new EdgeModelStatusDto
                {
                    ModelName = modelName,
                    Version = version,
                    IsLoaded = modelInfo.IsLoaded,
                    FilePath = modelInfo.FullFilePath,
                    LastLoadAttempt = modelInfo.Metadata?.DeploymentTimestamp ?? DateTime.MinValue // Example
                });
            }
            _logger.LogWarning("Status requested for model {ModelKey} which is not tracked.", modelKey);
            return Task.FromResult<EdgeModelStatusDto?>(null);
        }
    }
}