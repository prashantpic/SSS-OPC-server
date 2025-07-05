using AiService.Application.Dtos;

namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Defines the contract for a machine learning prediction engine.
/// </summary>
public interface IPredictionEngine
{
    /// <summary>
    /// Runs inference using a given model and input data.
    /// </summary>
    /// <param name="modelStream">A stream containing the model data (e.g., ONNX file).</param>
    /// <param name="inputData">A dictionary of input feature names and their values.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the prediction result.</returns>
    Task<PredictionResultDto> RunPredictionAsync(Stream modelStream, Dictionary<string, float> inputData, CancellationToken cancellationToken);
}