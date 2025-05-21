using OPC.Client.Core.Infrastructure.EdgeAI.Models; // For OnnxModelInput, OnnxModelOutput
using System.Threading.Tasks;

namespace OPC.Client.Core.Infrastructure.EdgeAI
{
    /// <summary>
    /// Interface for running ONNX (Open Neural Network Exchange) models at the edge.
    /// </summary>
    /// <remarks>
    /// Defines a contract for executing ONNX models, abstracting the underlying ONNX Runtime
    /// and facilitating edge AI processing.
    /// Implements REQ-7-001, REQ-8-001.
    /// </remarks>
    public interface IOnnxModelRunner
    {
        /// <summary>
        /// Asynchronously loads (if not already cached) and runs inference on an ONNX model.
        /// </summary>
        /// <param name="modelPath">The file path to the ONNX model.</param>
        /// <param name="input">The input data for the model, encapsulated in <see cref="OnnxModelInput"/>.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// the model's output, encapsulated in <see cref="OnnxModelOutput"/>.
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Thrown if the model file at <paramref name="modelPath"/> is not found.</exception>
        /// <exception cref="Microsoft.ML.OnnxRuntime.OnnxRuntimeException">Thrown if there is an error during model loading or inference.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="modelPath"/> or <paramref name="input"/> is null.</exception>
        Task<OnnxModelOutput> RunAsync(string modelPath, OnnxModelInput input);

        /// <summary>
        /// Checks if a model is already loaded and available for inference.
        /// </summary>
        /// <param name="modelPath">The file path to the ONNX model.</param>
        /// <returns>True if the model is loaded, false otherwise.</returns>
        bool IsModelLoaded(string modelPath);

        /// <summary>
        /// Preloads an ONNX model into memory for faster subsequent inferences.
        /// </summary>
        /// <param name="modelPath">The file path to the ONNX model.</param>
        /// <returns>A task representing the asynchronous preloading operation.
        /// True if successful, false otherwise.</returns>
        Task<bool> PreloadModelAsync(string modelPath);

        /// <summary>
        /// Unloads a previously loaded ONNX model from memory.
        /// </summary>
        /// <param name="modelPath">The file path to the ONNX model to unload.</param>
        void UnloadModel(string modelPath);
    }
}