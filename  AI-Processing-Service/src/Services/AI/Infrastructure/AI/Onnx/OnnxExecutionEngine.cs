using AIService.Domain.Enums;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Infrastructure.AI.Common;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIService.Infrastructure.AI.Onnx
{
    public class OnnxExecutionEngine : IModelExecutionEngine
    {
        private readonly ILogger<OnnxExecutionEngine> _logger;
        private readonly ModelFileLoader _modelFileLoader;
        private InferenceSession _session;
        private AiModel _modelDetails;

        public OnnxExecutionEngine(ILogger<OnnxExecutionEngine> logger, ModelFileLoader modelFileLoader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelFileLoader = modelFileLoader ?? throw new ArgumentNullException(nameof(modelFileLoader));
        }

        public ModelFormat SupportedFormat => ModelFormat.ONNX;

        public bool CanHandle(AiModel model)
        {
            return model?.ModelFormat == SupportedFormat;
        }

        public async Task LoadModelAsync(AiModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.ModelFormat != SupportedFormat)
                throw new ArgumentException($"Model format {model.ModelFormat} is not supported by {nameof(OnnxExecutionEngine)}.", nameof(model));

            _modelDetails = model;
            _logger.LogInformation("Loading ONNX model: {ModelName} Version: {ModelVersion} from {StorageReference}", model.Name, model.Version, model.StorageReference);

            try
            {
                var modelStream = await _modelFileLoader.LoadModelFileAsync(model.StorageReference);
                if (modelStream == null || modelStream.Length == 0)
                {
                    throw new FileNotFoundException($"Model file could not be loaded from {model.StorageReference}");
                }

                byte[] modelBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await modelStream.CopyToAsync(memoryStream);
                    modelBytes = memoryStream.ToArray();
                }
                await modelStream.DisposeAsync();

                // TODO: Consider SessionOptions for performance tuning (e.g., GPU execution if supported and configured)
                _session = new InferenceSession(modelBytes);
                _logger.LogInformation("ONNX model {ModelName} loaded successfully. Input Names: {InputNames}, Output Names: {OutputNames}",
                    model.Name,
                    string.Join(", ", _session.InputMetadata.Keys),
                    string.Join(", ", _session.OutputMetadata.Keys));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ONNX model {ModelName} from {StorageReference}", model.Name, model.StorageReference);
                throw;
            }
        }

        public Task<ModelOutput> ExecuteAsync(ModelInput input)
        {
            if (_session == null)
            {
                _logger.LogError("ONNX model session is not initialized. Load the model first.");
                throw new InvalidOperationException("Model not loaded. Call LoadModelAsync first.");
            }

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            _logger.LogDebug("Executing ONNX model {ModelName} with input.", _modelDetails.Name);

            try
            {
                var inputContainer = new List<NamedOnnxValue>();

                // This is a simplified input mapping. Real-world scenarios require robust mapping
                // based on _modelDetails.InputSchema and _session.InputMetadata.
                // For example, if ModelInput.Features is Dictionary<string, object>
                foreach (var feature in input.Features)
                {
                    if (!_session.InputMetadata.ContainsKey(feature.Key))
                    {
                        _logger.LogWarning("Input feature '{FeatureKey}' not found in model's input metadata. Skipping.", feature.Key);
                        continue;
                    }

                    var modelInputMetadata = _session.InputMetadata[feature.Key];
                    Tensor<float> tensor = null; // Default to float, adjust based on actual model needs and schema

                    // Example: Assuming input feature.Value is float[] and model expects Tensor<float>
                    if (feature.Value is float[] floatArray)
                    {
                        // Dimensions need to be derived from modelInputMetadata.Dimensions or schema
                        // For simplicity, assuming a 1D tensor or a 2D tensor with batch size 1
                        var dimensions = modelInputMetadata.Dimensions.Select(d => d == -1 ? 1 : d).ToArray();
                        if (dimensions.Length == 0 && floatArray.Length > 0) dimensions = new[] { 1, floatArray.Length }; // Heuristic
                        else if (dimensions.Length == 1 && dimensions[0] == 0 && floatArray.Length > 0) dimensions[0] = floatArray.Length;


                        // Ensure the total number of elements matches
                        long expectedElements = 1;
                        foreach (var dim in dimensions) expectedElements *= dim;
                        if (floatArray.Length != expectedElements && dimensions.Length > 0)
                        {
                             // Try to reshape if dimensions has -1
                            var unknownDimIndex = Array.IndexOf(dimensions, -1);
                            if (unknownDimIndex != -1)
                            {
                                long productOfKnownDims = 1;
                                for(int i=0; i<dimensions.Length; i++) if(i != unknownDimIndex) productOfKnownDims *= dimensions[i];
                                if (floatArray.Length % productOfKnownDims == 0)
                                {
                                    dimensions[unknownDimIndex] = (int)(floatArray.Length / productOfKnownDims);
                                    expectedElements = floatArray.Length; // Recalculate
                                }
                            }

                            if (floatArray.Length != expectedElements)
                            {
                                 _logger.LogError("Mismatched elements for feature '{FeatureKey}'. Expected {ExpectedElements}, got {ActualElements}. Dimensions: [{Dimensions}]",
                                    feature.Key, expectedElements, floatArray.Length, string.Join(",", dimensions));
                                throw new ArgumentException($"Mismatched elements for input '{feature.Key}'. Model expects {expectedElements} elements based on dimensions [{string.Join(",", dimensions)}], but got {floatArray.Length}.");
                            }
                        }
                         try
                        {
                            tensor = new DenseTensor<float>(floatArray, dimensions);
                        }
                        catch (Exception ex)
                        {
                             _logger.LogError(ex, "Error creating DenseTensor for feature '{FeatureKey}'. Dimensions: [{Dimensions}], Data Length: {DataLength}",
                                feature.Key, string.Join(",", dimensions), floatArray.Length);
                            throw;
                        }
                    }
                    // Add more type conversions as needed (e.g., double[], long[], string tensors for NLP)
                    // This requires parsing _modelDetails.InputSchema
                    else
                    {
                        _logger.LogError("Unsupported input type for feature '{FeatureKey}': {FeatureType}", feature.Key, feature.Value.GetType().Name);
                        throw new NotSupportedException($"Input type {feature.Value.GetType().Name} for feature '{feature.Key}' is not supported by this simplified ONNX engine.");
                    }
                    inputContainer.Add(NamedOnnxValue.CreateFromTensor(feature.Key, tensor));
                }


                using var results = _session.Run(inputContainer);
                var outputData = new Dictionary<string, object>();

                foreach (var outputMeta in _session.OutputMetadata)
                {
                    var resultValue = results.FirstOrDefault(r => r.Name == outputMeta.Key);
                    if (resultValue != null)
                    {
                        // This is simplified. Actual parsing depends on the output type and _modelDetails.OutputSchema
                        if (resultValue.Value is DenseTensor<float> floatTensor)
                        {
                            outputData[outputMeta.Key] = floatTensor.ToArray();
                        }
                        else if (resultValue.Value is DenseTensor<long> longTensor) // Example for classification output
                        {
                            outputData[outputMeta.Key] = longTensor.ToArray();
                        }
                        else if (resultValue.Value is IEnumerable<IDictionary<string, float>> probabilities) // For sequence of maps (common in classification)
                        {
                           outputData[outputMeta.Key] = probabilities.ToList();
                        }
                        else if (resultValue.Value is IEnumerable<IDictionary<long, float>> longKeyProbabilities)
                        {
                           outputData[outputMeta.Key] = longKeyProbabilities.ToList();
                        }
                         else if (resultValue.Value is IEnumerable<string> stringEnumerable)
                        {
                            outputData[outputMeta.Key] = stringEnumerable.ToList();
                        }
                        // Add more type handling as needed
                        else
                        {
                            _logger.LogWarning("Unhandled ONNX output type for {OutputName}: {OutputType}", outputMeta.Key, resultValue.Value.GetType().Name);
                            outputData[outputMeta.Key] = resultValue.Value; // Store as is, might need further processing
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Output {OutputName} not found in ONNX session results.", outputMeta.Key);
                    }
                }

                _logger.LogDebug("ONNX model {ModelName} execution completed.", _modelDetails.Name);
                return Task.FromResult(new ModelOutput { Outputs = outputData, RawOutput = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ONNX model {ModelName}", _modelDetails.Name);
                throw;
            }
        }

        public void Dispose()
        {
            _session?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}