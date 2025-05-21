using AIService.Domain.Enums;
using AIService.Domain.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Domain.Interfaces
{
    /// <summary>
    /// Defines the contract for a specific AI model runtime (e.g., ONNX, TensorFlow).
    /// Implementations will handle loading and running models of a particular format.
    /// </summary>
    public interface IModelExecutionEngine
    {
        /// <summary>
        /// Checks if this engine can handle the specified model format.
        /// </summary>
        /// <param name="format">The model format to check.</param>
        /// <returns>True if the engine supports the format, false otherwise.</returns>
        bool CanHandle(ModelFormat format);

        /// <summary>
        /// Loads the specified AI model artifact into the engine.
        /// This method should make the engine ready for execution.
        /// Implementations might cache loaded models internally.
        /// </summary>
        /// <param name="model">The AiModel metadata.</param>
        /// <param name="modelArtifactStream">A stream containing the model's binary artifact.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task LoadModelAsync(AiModel model, Stream modelArtifactStream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the pre-loaded model with the given input.
        /// </summary>
        /// <param name="modelId">Identifier of the model to execute (used for internal state management if engine supports multiple loaded models).</param>
        /// <param name="input">The ModelInput data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A ModelOutput containing the prediction results.</returns>
        /// <exception cref="InvalidOperationException">If the specified model is not loaded or if the engine is not ready.</exception>
        Task<ModelOutput> ExecuteAsync(string modelId, ModelInput input, CancellationToken cancellationToken = default);
    }
}