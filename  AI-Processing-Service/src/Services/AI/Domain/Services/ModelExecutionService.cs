using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Domain.Services
{
    /// <summary>
    /// Orchestrates the execution of an AiModel. It selects the appropriate
    /// IModelExecutionEngine based on the AiModel's format, loads the model
    /// (if not already loaded/cached), prepares input, executes, and processes output.
    /// </summary>
    public class ModelExecutionService
    {
        private readonly IModelRepository _modelRepository;
        private readonly IEnumerable<IModelExecutionEngine> _executionEngines;
        private readonly ILogger<ModelExecutionService> _logger;
        
        // Simple cache for loaded model artifacts or engine instances.
        // Key: (modelId, version). Value could be the loaded engine or a specific model object.
        // For simplicity, let's assume engines handle their own internal model state once LoadModelAsync is called.
        // This cache could store a reference to the engine instance that has loaded a specific model.
        // However, a single engine instance might be stateful for ONE model.
        // A better cache might be (modelId, version) -> Task<IModelExecutionEngine> where the task represents the loaded engine.
        // For now, we will resolve and load on each call, engines should implement their own caching if beneficial.
        // This service ensures the right engine is chosen and given the artifact.

        public ModelExecutionService(
            IModelRepository modelRepository,
            IEnumerable<IModelExecutionEngine> executionEngines,
            ILogger<ModelExecutionService> logger)
        {
            _modelRepository = modelRepository ?? throw new ArgumentNullException(nameof(modelRepository));
            _executionEngines = executionEngines ?? throw new ArgumentNullException(nameof(executionEngines));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ModelOutput> ExecuteModelAsync(string modelId, string version, ModelInput input, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                throw new ArgumentException("Model ID cannot be null or whitespace.", nameof(modelId));
            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Model version cannot be null or whitespace.", nameof(version));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            _logger.LogInformation("Attempting to execute model {ModelId} version {Version}.", modelId, version);

            AiModel? model = await _modelRepository.GetModelAsync(modelId, cancellationToken);
            if (model == null)
            {
                _logger.LogError("Model metadata not found for ID {ModelId}.", modelId);
                return new ModelOutput(new Dictionary<string, object>(), false, $"Model metadata not found for ID {modelId}.");
            }
            
            // Ensure the version matches if the model ID itself isn't version-specific in GetModelAsync
            // Or, adjust GetModelAsync to fetch specific version or GetModelByNameAndVersionAsync
            // Assuming GetModelAsync(modelId) returns the base model, and we then check version compatibility or StorageReference for version
            // For this example, let's assume the model version is part of the modelId or handled by repository implicitly if it returns a specific version.
            // If AiModel.Version needs to be explicitly matched:
            if (model.Version != version)
            {
                 _logger.LogWarning("Requested model version {RequestedVersion} does not match stored model version {StoredVersion} for ID {ModelId}. Attempting to fetch specific version artifact.", version, model.Version, modelId);
                 // Potentially, the GetModelAsync should be more specific or a GetModelVersionAsync method is needed.
                 // For now, we proceed assuming the StorageReference on `model` is for the requested `version`.
            }


            IModelExecutionEngine? engine = _executionEngines.FirstOrDefault(e => e.CanHandle(model.ModelFormat));
            if (engine == null)
            {
                _logger.LogError("No execution engine found for model format {ModelFormat} of model {ModelId}.", model.ModelFormat, modelId);
                return new ModelOutput(new Dictionary<string, object>(), false, $"No execution engine for format {model.ModelFormat}.");
            }

            try
            {
                // TODO: Implement input schema validation against model.InputSchema
                // E.g., using a JSON schema validator if InputSchema is JSON.
                // For now, we pass it through.

                Stream? modelArtifactStream = await _modelRepository.GetModelArtifactStreamAsync(modelId, version, cancellationToken);
                if (modelArtifactStream == null)
                {
                    _logger.LogError("Model artifact not found for model {ModelId} version {Version}.", modelId, version);
                    return new ModelOutput(new Dictionary<string, object>(), false, $"Model artifact not found for {modelId} v{version}.");
                }

                using (modelArtifactStream)
                {
                    await engine.LoadModelAsync(model, modelArtifactStream, cancellationToken);
                }
                
                ModelOutput output = await engine.ExecuteAsync(model.Id, input, cancellationToken); // Pass model.Id for engine's internal state if needed

                _logger.LogInformation("Successfully executed model {ModelId} version {Version}.", modelId, version);
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing model {ModelId} version {Version}.", modelId, version);
                return new ModelOutput(new Dictionary<string, object>(), false, $"Execution error: {ex.Message}");
            }
        }
    }
}