namespace AIService.Domain.Services
{
    using AIService.Domain.Interfaces;
    using AIService.Domain.Models;
    using AIService.Domain.Enums;
    using Microsoft.Extensions.Logging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.IO;
    using System.Collections.Concurrent;
    using System;

    /// <summary>
    /// Orchestrates the execution of an AiModel. It selects the appropriate
    /// IModelExecutionEngine, loads the model, prepares input, executes, and processes output.
    /// Implements caching for loaded model instances.
    /// (REQ-7-003, REQ-8-001)
    /// </summary>
    public class ModelExecutionService
    {
        private readonly IModelRepository _modelRepository;
        private readonly IEnumerable<IModelExecutionEngine> _executionEngines;
        private readonly ILogger<ModelExecutionService> _logger;
        private readonly ConcurrentDictionary<string, string> _loadedModelCache; // Key: modelId:version, Value: loadedModelIdentifier from engine

        public ModelExecutionService(
            IModelRepository modelRepository,
            IEnumerable<IModelExecutionEngine> executionEngines,
            ILogger<ModelExecutionService> logger)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _executionEngines = executionEngines ?? throw new ArgumentNullException(nameof(executionEngines));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loadedModelCache = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Executes a specified AI model with the given input.
        /// </summary>
        /// <param name="modelId">The ID of the AiModel to execute.</param>
        /// <param name="modelVersion">The version of the AiModel to execute. If null, attempts to use a default or latest version.</param>
        /// <param name="input">The ModelInput data for the execution.</param>
        /// <returns>A ModelOutput containing the prediction results.</returns>
        public async Task<ModelOutput> ExecuteModelAsync(string modelId, string modelVersion, ModelInput input)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                _logger.LogError("Model ID cannot be null or whitespace.");
                return new ModelOutput { IsSuccess = false, ErrorMessage = "Model ID is required." };
            }
            if (input == null)
            {
                _logger.LogError("Model input cannot be null for model ID {ModelId}.", modelId);
                return new ModelOutput { IsSuccess = false, ErrorMessage = "Model input is required." };
            }

            _logger.LogInformation("Attempting to execute model ID: {ModelId}, Version: {ModelVersion}", modelId, modelVersion ?? "Default/Latest");

            AiModel modelDefinition = await _modelRepository.GetModelAsync(modelId, modelVersion);
            if (modelDefinition == null)
            {
                _logger.LogError("Model definition not found for ID: {ModelId}, Version: {ModelVersion}", modelId, modelVersion);
                return new ModelOutput { IsSuccess = false, ErrorMessage = $"Model {modelId} (Version: {modelVersion ?? "any"}) not found." };
            }

            IModelExecutionEngine engine = _executionEngines.FirstOrDefault(e => e.SupportedFormat == modelDefinition.ModelFormat);
            if (engine == null)
            {
                _logger.LogError("No execution engine found for model format: {ModelFormat} (Model ID: {ModelId})", modelDefinition.ModelFormat, modelId);
                return new ModelOutput { IsSuccess = false, ErrorMessage = $"Unsupported model format: {modelDefinition.ModelFormat}" };
            }

            _logger.LogDebug("Using engine for format {ModelFormat} for model ID: {ModelId}", modelDefinition.ModelFormat, modelId);

            string cacheKey = $"{modelDefinition.Id}:{modelDefinition.Version}";
            string loadedModelIdentifier;

            if (!_loadedModelCache.TryGetValue(cacheKey, out loadedModelIdentifier))
            {
                _logger.LogInformation("Model {CacheKey} not found in cache. Loading...", cacheKey);
                try
                {
                    using (Stream modelArtifactStream = await _modelRepository.GetModelArtifactStreamAsync(modelDefinition.Id, modelDefinition.Version))
                    {
                        if (modelArtifactStream == null)
                        {
                             _logger.LogError("Model artifact stream not found for ID: {ModelId}, Version: {ModelVersion}", modelId, modelDefinition.Version);
                            return new ModelOutput { IsSuccess = false, ErrorMessage = $"Model artifact for {modelId} (Version: {modelDefinition.Version}) not found." };
                        }
                        loadedModelIdentifier = await engine.LoadModelAsync(modelArtifactStream, modelDefinition);
                        _loadedModelCache.TryAdd(cacheKey, loadedModelIdentifier);
                        _logger.LogInformation("Model {CacheKey} loaded successfully with identifier {LoadedModelIdentifier}.", cacheKey, loadedModelIdentifier);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load model ID: {ModelId}, Version: {ModelVersion}. Error: {ErrorMessage}", modelId, modelDefinition.Version, ex.Message);
                    return new ModelOutput { IsSuccess = false, ErrorMessage = $"Failed to load model: {ex.Message}" };
                }
            }
            else
            {
                 _logger.LogInformation("Model {CacheKey} found in cache with identifier {LoadedModelIdentifier}.", cacheKey, loadedModelIdentifier);
            }
            
            // TODO: Add input schema validation against modelDefinition.InputSchema here before execution.

            try
            {
                _logger.LogDebug("Executing model {CacheKey} with identifier {LoadedModelIdentifier}.", cacheKey, loadedModelIdentifier);
                ModelOutput output = await engine.ExecuteAsync(loadedModelIdentifier, input);
                _logger.LogInformation("Model {CacheKey} execution completed.", cacheKey);
                // TODO: Add output schema validation/transformation against modelDefinition.OutputSchema if needed.
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing model ID: {ModelId}, Version: {ModelVersion}. Error: {ErrorMessage}", modelId, modelDefinition.Version, ex.Message);
                return new ModelOutput { IsSuccess = false, ErrorMessage = $"Error during model execution: {ex.Message}" };
            }
        }

        /// <summary>
        /// Unloads a specific model version from the cache and the execution engine.
        /// </summary>
        /// <param name="modelId">The ID of the model to unload.</param>
        /// <param name="modelVersion">The version of the model to unload.</param>
        public async Task UnloadModelAsync(string modelId, string modelVersion)
        {
            string cacheKey = $"{modelId}:{modelVersion}";
            if (_loadedModelCache.TryRemove(cacheKey, out string loadedModelIdentifier))
            {
                AiModel modelDefinition = await _modelRepository.GetModelAsync(modelId, modelVersion); // Re-fetch for format
                if (modelDefinition != null)
                {
                    IModelExecutionEngine engine = _executionEngines.FirstOrDefault(e => e.SupportedFormat == modelDefinition.ModelFormat);
                    if (engine != null)
                    {
                        try
                        {
                            await engine.UnloadModelAsync(loadedModelIdentifier);
                            _logger.LogInformation("Model {CacheKey} with identifier {LoadedModelIdentifier} unloaded successfully.", cacheKey, loadedModelIdentifier);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error unloading model {CacheKey} with identifier {LoadedModelIdentifier} from engine.", cacheKey, loadedModelIdentifier);
                        }
                    }
                    else
                    {
                         _logger.LogWarning("No engine found to unload model {CacheKey} format {ModelFormat}.", cacheKey, modelDefinition.ModelFormat);
                    }
                }
                 else
                {
                    _logger.LogWarning("Could not find model definition for {CacheKey} during unload, engine unload skipped for identifier {LoadedModelIdentifier}.", cacheKey, loadedModelIdentifier);
                }
            }
            else
            {
                _logger.LogInformation("Model {CacheKey} was not found in the loaded model cache for unloading.", cacheKey);
            }
        }
    }
}