namespace Opc.Client.Core.Application.Interfaces;

/// <summary>
/// Provides a contract for loading and executing AI models on an edge device.
/// </summary>
/// <remarks>
/// This interface abstracts the specifics of the AI model execution runtime (e.g., ONNX Runtime),
/// allowing the application to interact with a simple, high-level interface for running inferences.
/// </remarks>
public interface IAiModelRunner
{
    /// <summary>
    /// Asynchronously loads an AI model from a specified file path.
    /// </summary>
    /// <param name="modelPath">The file path to the AI model (e.g., an .onnx file).</param>
    /// <returns>A task that represents the asynchronous loading operation.</returns>
    Task LoadModelAsync(string modelPath);

    /// <summary>
    /// Asynchronously runs an inference using the loaded model.
    /// </summary>
    /// <param name="inputData">The input data for the model.</param>
    /// <returns>
    /// A task that represents the asynchronous inference operation, 
    /// containing the structured output data from the model.
    /// </returns>
    Task<ModelOutputData> RunInferenceAsync(ModelInputData inputData);
}

// --- Placeholder types for compilation ---

/// <summary>
/// Represents the structured input data for an AI model inference.
/// </summary>
/// <param name="Features">A collection of feature values for the model.</param>
public record ModelInputData(IReadOnlyDictionary<string, object> Features);


/// <summary>
/// Represents the structured output data from an AI model inference.
/// </summary>
/// <param name="Results">A collection of results produced by the model.</param>
public record ModelOutputData(IReadOnlyDictionary<string, object> Results);