namespace AIService.Domain.Interfaces
{
    using AIService.Domain.Enums;
    using AIService.Domain.Models;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for a specific AI model runtime (e.g., ONNX, TensorFlow).
    /// Implementations will handle loading and running models of a particular format.
    /// (REQ-7-003, REQ-8-001)
    /// </summary>
    public interface IModelExecutionEngine
    {
        /// <summary>
        /// Gets the model format that this engine can handle.
        /// </summary>
        ModelFormat SupportedFormat { get; }

        /// <summary>
        /// Loads an AI model from a stream into the execution engine.
        /// The AiModel definition provides context like input/output schema.
        /// </summary>
        /// <param name="modelArtifactStream">A stream containing the model artifact.</param>
        /// <param name="modelDefinition">The AiModel definition providing metadata and schema.</param>
        /// <returns>A task representing the asynchronous loading operation. Returns a unique identifier for the loaded model instance or session.</returns>
        Task<string> LoadModelAsync(Stream modelArtifactStream, AiModel modelDefinition);
        
        /// <summary>
        /// Unloads a previously loaded model instance to free up resources.
        /// </summary>
        /// <param name="loadedModelIdentifier">The identifier of the loaded model instance, returned by LoadModelAsync.</param>
        /// <returns>A task representing the asynchronous unloading operation.</returns>
        Task UnloadModelAsync(string loadedModelIdentifier);

        /// <summary>
        /// Executes a loaded AI model with the given input.
        /// </summary>
        /// <param name="loadedModelIdentifier">The identifier of the loaded model instance.</param>
        /// <param name="input">The ModelInput data for the execution.</param>
        /// <returns>A ModelOutput containing the prediction results.</returns>
        Task<ModelOutput> ExecuteAsync(string loadedModelIdentifier, ModelInput input);
    }
}