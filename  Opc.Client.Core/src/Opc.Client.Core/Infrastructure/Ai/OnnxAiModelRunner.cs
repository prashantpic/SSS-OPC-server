using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Opc.Client.Core.Application.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Opc.Client.Core.Infrastructure.Ai;

/// <summary>
/// A concrete implementation of IAiModelRunner that uses the ONNX Runtime for model execution.
/// </summary>
/// <remarks>
/// This class encapsulates the logic for loading an ONNX model, preparing input tensors,
/// running an inference session, and parsing the output tensors back into domain objects.
/// </remarks>
public class OnnxAiModelRunner : IAiModelRunner, IDisposable
{
    private InferenceSession? _inferenceSession;

    /// <inheritdoc/>
    public Task LoadModelAsync(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            throw new FileNotFoundException("ONNX model file not found.", modelPath);
        }

        // Dispose previous session if one exists
        _inferenceSession?.Dispose();

        // Consider SessionOptions for performance tuning (e.g., GPU execution)
        var sessionOptions = new SessionOptions();
        _inferenceSession = new InferenceSession(modelPath, sessionOptions);
        
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<ModelOutputData> RunInferenceAsync(ModelInputData inputData)
    {
        if (_inferenceSession == null)
        {
            throw new InvalidOperationException("Model has not been loaded. Call LoadModelAsync first.");
        }

        var inputs = new List<NamedOnnxValue>();
        try
        {
            // This logic assumes the ONNX model's input names match the keys in the inputData dictionary.
            foreach (var inputNode in _inferenceSession.InputMetadata)
            {
                var nodeName = inputNode.Key;
                if (!inputData.Features.TryGetValue(nodeName, out var value))
                {
                    throw new ArgumentException($"Input data is missing a value for model input: {nodeName}");
                }
                
                // This part needs to be robust to handle different data types and shapes.
                // Example for a float array input.
                if (value is float[] floatArray)
                {
                    // ONNX models often expect a shape like [batch_size, num_features]
                    // Here we assume batch_size = 1.
                    var dimensions = new int[] { 1, floatArray.Length };
                    var tensor = new DenseTensor<float>(floatArray, dimensions);
                    inputs.Add(NamedOnnxValue.CreateFromTensor(nodeName, tensor));
                }
                else
                {
                    // Add more type handling as needed (long, double, string, etc.)
                    throw new NotSupportedException($"Input type {value.GetType()} is not supported for ONNX conversion.");
                }
            }

            using var results = _inferenceSession.Run(inputs);

            var outputDictionary = results.ToDictionary(
                r => r.Name,
                r => r.Value as object); // The value will be an OnnxTensor or OnnxSequence

            var modelOutput = new ModelOutputData(outputDictionary);
            return Task.FromResult(modelOutput);
        }
        finally
        {
            // Dispose tensors created
            foreach (var item in inputs)
            {
                item.Dispose();
            }
        }
    }

    public void Dispose()
    {
        _inferenceSession?.Dispose();
        GC.SuppressFinalize(this);
    }
}