using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using services.opc.client.Domain.Abstractions;
using services.opc.client.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace services.opc.client.Infrastructure.EdgeProcessing;

/// <summary>
/// Concrete implementation of IEdgeAiRuntime using Microsoft.ML.OnnxRuntime.
/// This class loads and executes AI models in the ONNX format locally.
/// </summary>
public class OnnxAiRuntime : IEdgeAiRuntime, IDisposable
{
    private readonly ILogger<OnnxAiRuntime> _logger;
    private InferenceSession? _inferenceSession;

    public OnnxAiRuntime(ILogger<OnnxAiRuntime> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task LoadModelAsync(string modelPath)
    {
        if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
        {
            _logger.LogError("ONNX model file not found at path: {ModelPath}", modelPath);
            throw new FileNotFoundException("ONNX model file not found.", modelPath);
        }

        try
        {
            _logger.LogInformation("Loading ONNX model from {ModelPath}", modelPath);
            _inferenceSession = new InferenceSession(modelPath);
            _logger.LogInformation("ONNX model loaded successfully. Input names: {InputNames}. Output names: {OutputNames}",
                string.Join(", ", _inferenceSession.InputNames),
                string.Join(", ", _inferenceSession.OutputNames));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load ONNX model from {ModelPath}", modelPath);
            throw;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<ModelOutput> RunInferenceAsync(ModelInput input)
    {
        if (_inferenceSession == null)
        {
            throw new InvalidOperationException("Model is not loaded. Call LoadModelAsync first.");
        }

        try
        {
            var inputTensor = new DenseTensor<float>(input.Features, new[] { 1, input.Features.Length });
            
            // Assuming the model has a single input named "input"
            var inputName = _inferenceSession.InputNames.FirstOrDefault();
            if(inputName == null)
            {
                throw new InvalidOperationException("Could not determine input name from the ONNX model.");
            }

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, inputTensor)
            };

            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _inferenceSession.Run(inputs);

            var outputTensor = results.FirstOrDefault()?.AsTensor<float>();
            if (outputTensor == null)
            {
                throw new InvalidOperationException("Inference did not produce a valid output tensor.");
            }

            var modelOutput = new ModelOutput(outputTensor.ToArray());
            return Task.FromResult(modelOutput);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during ONNX model inference.");
            throw;
        }
    }

    /// <summary>
    /// Disposes the inference session.
    /// </summary>
    public void Dispose()
    {
        _inferenceSession?.Dispose();
        GC.SuppressFinalize(this);
    }
}