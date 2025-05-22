using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using IndustrialAutomation.OpcClient.Application.Interfaces;
using IndustrialAutomation.OpcClient.Infrastructure.EdgeIntelligence;
using IndustrialAutomation.OpcClient.Infrastructure.DataHandling;
using IndustrialAutomation.OpcClient.Domain.Models; // For BufferedDataItem
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // For model path

namespace IndustrialAutomation.OpcClient.Application.Services
{
    public class EdgeIntelligenceService : IEdgeIntelligenceService
    {
        private readonly ILogger<EdgeIntelligenceService> _logger;
        private readonly IEdgeAiExecutor _aiExecutor;
        private readonly IModelRepository _modelRepository;
        private readonly IDataTransmissionService _dataTransmissionService; // To send AI output
        private readonly IDataBufferer _dataBufferer; // To buffer AI output if transmission fails
        private readonly string _defaultModelPath;

        private readonly ConcurrentDictionary<string, EdgeModelMetadataDto> _loadedModelsMetadata = new();

        public EdgeIntelligenceService(
            ILogger<EdgeIntelligenceService> logger,
            IEdgeAiExecutor aiExecutor,
            IModelRepository modelRepository,
            IDataTransmissionService dataTransmissionService,
            IDataBufferer dataBufferer,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _aiExecutor = aiExecutor ?? throw new ArgumentNullException(nameof(aiExecutor));
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _dataTransmissionService = dataTransmissionService ?? throw new ArgumentNullException(nameof(dataTransmissionService));
            _dataBufferer = dataBufferer ?? throw new ArgumentNullException(nameof(dataBufferer));
            _defaultModelPath = configuration["EdgeAI:ModelPath"] ?? "models"; // Get from config
        }

        public async Task LoadModelAsync(string modelName, string version)
        {
            if (string.IsNullOrWhiteSpace(modelName)) throw new ArgumentNullException(nameof(modelName));
            if (string.IsNullOrWhiteSpace(version)) version = "latest"; // Or handle as error

            _logger.LogInformation("Attempting to load AI model: {ModelName} version {Version}", modelName, version);
            try
            {
                // IModelRepository.LoadModelFilePath was in instruction, but SDS had IModelRepository.LoadModelAsync returning EdgeAiModel
                // The current IModelRepository.cs instruction has LoadModelFilePath, so we'll use that.
                // This means IEdgeAiExecutor.LoadModel needs the file path.
                
                var modelMetadata = await _modelRepository.GetModelMetadata(modelName, version);
                if (modelMetadata == null)
                {
                    // Attempt to load from default path if metadata not found and path is in metadata
                    // This logic might need refinement based on how models are discovered/registered.
                    // For now, assume GetModelMetadata provides a valid path or one can be constructed.
                    modelMetadata = new EdgeModelMetadataDto(
                        modelName, 
                        version, 
                        modelName, 
                        System.IO.Path.Combine(_defaultModelPath, modelName, version, $"{modelName}.onnx"), // Construct a plausible path
                        DateTime.UtcNow, 
                        new Dictionary<string, string>(), 
                        new Dictionary<string, string>()
                        );

                    // If GetModelMetadata is meant to also check file existence or get it from a manifest,
                    // then this fallback might not be needed, or GetModelMetadata should throw.
                    // _logger.LogWarning("Metadata not found for model {ModelName} version {Version}. Attempting to construct path.", modelName, version);
                }
                
                string modelFilePath = modelMetadata.FilePath;
                if (string.IsNullOrEmpty(modelFilePath) || !System.IO.File.Exists(modelFilePath))
                {
                     modelFilePath = await _modelRepository.LoadModelFilePath(modelName, version); // Try this one too
                     if (string.IsNullOrEmpty(modelFilePath) || !System.IO.File.Exists(modelFilePath))
                     {
                        _logger.LogError("Model file path not found or file does not exist for {ModelName} version {Version} at path: {ModelPath}", modelName, version, modelFilePath);
                        return;
                     }
                }


                await _aiExecutor.LoadModel(modelFilePath); // IEdgeAiExecutor.LoadModel expects file path
                _loadedModelsMetadata[modelName] = modelMetadata; // Store metadata for the loaded model
                _logger.LogInformation("Successfully loaded AI model: {ModelName} version {Version} from {ModelFilePath}", modelName, version, modelFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load AI model: {ModelName} version {Version}", modelName, version);
            }
        }

        public async Task<EdgeModelOutputDto> ExecuteModelAsync(string modelName, EdgeModelInputDto input)
        {
            if (string.IsNullOrWhiteSpace(modelName)) throw new ArgumentNullException(nameof(modelName));
            if (input == null) throw new ArgumentNullException(nameof(input));

            if (!_loadedModelsMetadata.ContainsKey(modelName))
            {
                _logger.LogWarning("Model {ModelName} is not loaded. Attempting to load default version.", modelName);
                await LoadModelAsync(modelName, "latest"); // Attempt to load if not already
                if (!_loadedModelsMetadata.ContainsKey(modelName))
                {
                     _logger.LogError("ExecuteModelAsync failed: Model {ModelName} is not loaded and could not be loaded.", modelName);
                    return new EdgeModelOutputDto(modelName, input.ModelVersion ?? "unknown", DateTime.UtcNow, new Dictionary<string, object>(), "Error: Model not loaded", new Dictionary<string, object>());
                }
            }

            _logger.LogDebug("Executing AI model: {ModelName} with input features count: {FeatureCount}", modelName, input.Features.Count);
            try
            {
                var output = await _aiExecutor.Execute(modelName, input);
                _logger.LogInformation("Successfully executed AI model: {ModelName}. Output result count: {ResultCount}", modelName, output.Results.Count);

                // Send output to server
                await _dataTransmissionService.SendEdgeAiOutputAsync(output);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute AI model: {ModelName}", modelName);
                return new EdgeModelOutputDto(modelName, input.ModelVersion ?? "unknown", DateTime.UtcNow, new Dictionary<string, object>(), $"Error: {ex.Message}", new Dictionary<string, object>());
            }
        }

        public async Task HandleModelUpdateAsync(EdgeModelMetadataDto metadata, byte[] modelBytes)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            if (modelBytes == null || modelBytes.Length == 0) throw new ArgumentNullException(nameof(modelBytes));

            _logger.LogInformation("Handling AI model update for: {ModelName} version {Version}", metadata.ModelName, metadata.Version);
            try
            {
                await _modelRepository.SaveModelAsync(metadata, modelBytes);
                _logger.LogInformation("Model {ModelName} version {Version} saved to repository.", metadata.ModelName, metadata.Version);

                // Now load the new model
                await LoadModelAsync(metadata.ModelName, metadata.Version); // This will use the path from saved metadata or constructed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle AI model update for: {ModelName} version {Version}", metadata.ModelName, metadata.Version);
            }
        }

        public Task<string> GetModelStatusAsync(string modelName)
        {
            if (_loadedModelsMetadata.TryGetValue(modelName, out var metadata))
            {
                // Could add more details like last execution time, error count etc.
                return Task.FromResult($"Model '{modelName}' (Version: {metadata.Version}) is loaded. Path: {metadata.FilePath}");
            }
            return Task.FromResult($"Model '{modelName}' is not loaded.");
        }
    }
}