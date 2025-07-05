using AiService.Application.Dtos;
using AiService.Application.Interfaces.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AiService.Infrastructure.MachineLearning.Onnx;

/// <summary>
/// Concrete implementation for model inference using ONNX Runtime.
/// </summary>
public class OnnxPredictionEngine : IPredictionEngine
{
    private readonly IMemoryCache _modelCache;
    private readonly ILogger<OnnxPredictionEngine> _logger;

    public OnnxPredictionEngine(IMemoryCache memoryCache, ILogger<OnnxPredictionEngine> logger)
    {
        _modelCache = memoryCache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PredictionResultDto> RunPredictionAsync(Stream modelStream, Dictionary<string, float> inputData, CancellationToken cancellationToken)
    {
        try
        {
            // Use a hash of the stream as a cache key. For simplicity, we'll use a Guid for this example.
            // A more robust implementation would involve hashing the stream content.
            var cacheKey = Guid.NewGuid().ToString(); 
            
            var session = await _modelCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                _logger.LogInformation("ONNX model not found in cache. Loading from stream.");
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                using var memoryStream = new MemoryStream();
                await modelStream.CopyToAsync(memoryStream, cancellationToken);
                return new InferenceSession(memoryStream.ToArray());
            });

            if (session is null)
            {
                throw new InvalidOperationException("Failed to load or retrieve ONNX Inference Session.");
            }

            // Assume the model has a single input node. The name must match the model's definition.
            var inputNodeName = session.InputMetadata.Keys.First();
            var inputShape = session.InputMetadata[inputNodeName].Dimensions.ToArray();
            
            // This assumes the model expects a shape like [1, N] where N is the number of features.
            if (inputShape.Length != 2 || inputShape[0] != 1)
            {
                throw new NotSupportedException("The ONNX model input shape is not supported. Expected shape [1, N].");
            }
            var featureCount = inputShape[1];

            // The order of features in inputData must match the order expected by the model.
            // A robust solution would require metadata about feature order. We assume it matches here.
            var inputTensorValues = inputData.Values.ToArray();
            if (inputTensorValues.Length != featureCount)
            {
                 throw new ArgumentException($"Input data has {inputTensorValues.Length} features but model expects {featureCount}.");
            }

            var inputTensor = new DenseTensor<float>(inputTensorValues, inputShape);
            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputNodeName, inputTensor) };

            _logger.LogDebug("Running ONNX inference session.");
            using var results = session.Run(inputs);
            
            // Assume the model has a single output.
            var output = results.FirstOrDefault();
            if (output == null)
            {
                throw new InvalidOperationException("ONNX model did not produce any output.");
            }

            // Assuming the output is a single float value (e.g., a regression prediction)
            var predictionValue = output.AsTensor<float>().GetValue(0);
            var metadata = new Dictionary<string, object>
            {
                { "OutputNodeName", output.Name }
            };

            return new PredictionResultDto(predictionValue, metadata);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in the ONNX prediction engine.");
            throw;
        }
    }
}