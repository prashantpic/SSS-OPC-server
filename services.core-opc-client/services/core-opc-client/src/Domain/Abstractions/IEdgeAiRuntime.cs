using services.opc.client.Domain.Models;

namespace services.opc.client.Domain.Abstractions;

/// <summary>
/// Defines the contract for the AI model execution engine.
/// This allows the application to run inference on models (e.g., ONNX)
/// without being tightly coupled to a specific AI runtime library.
/// </summary>
public interface IEdgeAiRuntime
{
    /// <summary>
    /// Loads an AI model from the specified file path.
    /// </summary>
    /// <param name="modelPath">The path to the model file (e.g., an .onnx file).</param>
    /// <returns>A task representing the asynchronous loading operation.</returns>
    Task LoadModelAsync(string modelPath);

    /// <summary>
    /// Runs inference using the loaded model.
    /// </summary>
    /// <param name="input">The input data for the model.</param>
    /// <returns>A task that resolves to the model's output.</returns>
    Task<ModelOutput> RunInferenceAsync(ModelInput input);
}