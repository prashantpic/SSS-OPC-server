using AIService.Domain.Enums;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Infrastructure.AI.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tensorflow;
using Tensorflow.Keras.Utils;
using Tensorflow.NumPy; // For NDArray

namespace AIService.Infrastructure.AI.TensorFlow
{
    public class TensorFlowExecutionEngine : IModelExecutionEngine
    {
        private readonly ILogger<TensorFlowExecutionEngine> _logger;
        private readonly ModelFileLoader _modelFileLoader;
        private Graph _graph;
        private Session _session;
        private AiModel _modelDetails;
        private bool _isTfLiteModel = false;

        // TODO: TensorFlow.NET setup might require specific initialization.
        // Ensure Graph.DefaultGraph is handled correctly if multiple models are loaded.

        public TensorFlowExecutionEngine(ILogger<TensorFlowExecutionEngine> logger, ModelFileLoader modelFileLoader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelFileLoader = modelFileLoader ?? throw new ArgumentNullException(nameof(modelFileLoader));
            // Initialize TensorFlow.NET specific settings if necessary
            // TF_API.꽤_Initialize(); // Example, check TensorFlow.NET documentation
        }

        public ModelFormat SupportedFormat => ModelFormat.TensorFlow; // Also handles TensorFlowLite

        public bool CanHandle(AiModel model)
        {
            return model?.ModelFormat == ModelFormat.TensorFlow || model?.ModelFormat == ModelFormat.TensorFlowLite;
        }

        public async Task LoadModelAsync(AiModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.ModelFormat != ModelFormat.TensorFlow && model.ModelFormat != ModelFormat.TensorFlowLite)
                throw new ArgumentException($"Model format {model.ModelFormat} is not supported by {nameof(TensorFlowExecutionEngine)}.", nameof(model));

            _modelDetails = model;
            _isTfLiteModel = model.ModelFormat == ModelFormat.TensorFlowLite;

            _logger.LogInformation("Loading TensorFlow model: {ModelName} Version: {ModelVersion} from {StorageReference}. Is TFLite: {IsTfLite}",
                model.Name, model.Version, model.StorageReference, _isTfLiteModel);

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

                _graph = new Graph();
                _session = new Session(_graph);

                if (_isTfLiteModel)
                {
                    // TensorFlow.NET's TFLite support might be through importing a graph def
                    // or direct interpreter interaction. The latter is more common for TFLite.
                    // As of current TensorFlow.NET, direct TFLite interpreter might not be fully mature.
                    // Assuming conversion to TF graph or that TensorFlow.NET has a TFLite graph loading mechanism.
                    // This part needs verification against the current state of TensorFlow.NET for TFLite.
                    // For now, let's assume TFLite models are loaded as frozen graphs if TensorFlow.NET supports it.
                    _graph.Import(modelBytes); // This is a guess for TFLite, might need specific TFLite APIs.
                    _logger.LogInformation("TensorFlow Lite model {ModelName} graph definition loaded (assuming frozen graph format).", model.Name);
                }
                else // SavedModel format
                {
                    // Loading SavedModel typically involves loading a directory.
                    // If 'modelBytes' is a .pb file (frozen graph), this is simpler.
                    // If it's a full SavedModel, ModelFileLoader would need to provide a path to the extracted directory.
                    // For simplicity, assuming `model.StorageReference` points to a .pb file for frozen graph
                    // or ModelFileLoader handles SavedModel directory structure.
                    // The `modelBytes` would typically be from a frozen graph .pb file.

                    // If _modelFileLoader gives a path to a SavedModel directory:
                    // var sm = SavedModelLoader.load(model.StorageReference, sess); // TensorFlow.NET API may vary
                    // _graph = sm.Graph;
                    // _session = sm.Session; // Or use the existing session

                    // If modelBytes is a .pb file (frozen graph):
                    _graph.Import(modelBytes);
                    _logger.LogInformation("TensorFlow SavedModel/frozen graph {ModelName} graph definition loaded.", model.Name);
                }

                // Log input/output nodes if discoverable
                // foreach (var op in _graph.get_operations()) { _logger.LogDebug($"Op: {op.name}"); }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading TensorFlow model {ModelName} from {StorageReference}", model.Name, model.StorageReference);
                throw;
            }
        }

        public Task<ModelOutput> ExecuteAsync(ModelInput input)
        {
            if (_session == null || _graph == null)
            {
                _logger.LogError("TensorFlow model session/graph is not initialized. Load the model first.");
                throw new InvalidOperationException("Model not loaded. Call LoadModelAsync first.");
            }

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            _logger.LogDebug("Executing TensorFlow model {ModelName} with input.", _modelDetails.Name);

            try
            {
                var feedDict = new FeedItem[_modelDetails.InputSchema.Features.Count];
                int i = 0;

                // Requires _modelDetails.InputSchema to map input.Features to tensor names and shapes.
                // Example: _modelDetails.InputSchema.Features might be a list of { Name, DataType, Shape }
                foreach (var schemaFeature in _modelDetails.InputSchema.Features)
                {
                    if (!input.Features.TryGetValue(schemaFeature.Name, out var featureValue))
                    {
                        _logger.LogError("Input feature '{FeatureName}' defined in schema not found in ModelInput.", schemaFeature.Name);
                        throw new ArgumentException($"Missing input feature: {schemaFeature.Name}");
                    }

                    // This is highly dependent on the specific model's input tensor names and types
                    // Assuming schemaFeature.Name corresponds to the tensor name (e.g., "input_1:0")
                    var inputTensor = _graph.OperationByName(schemaFeature.TensorName ?? schemaFeature.Name); // Or _graph.get_tensor_by_name(...)
                    if (inputTensor == null)
                    {
                        _logger.LogError("Could not find input tensor '{TensorName}' in the graph.", schemaFeature.TensorName ?? schemaFeature.Name);
                        throw new InvalidOperationException($"Input tensor '{schemaFeature.TensorName ?? schemaFeature.Name}' not found.");
                    }
                    
                    // Convert featureValue to NDArray based on schemaFeature.DataType and schemaFeature.Shape
                    // Simplified example assuming float[] input for a tensor expecting floats
                    if (featureValue is float[] floatArray)
                    {
                        // Shape needs to come from schemaFeature.Shape, e.g., {1, 224, 224, 3}
                        // For simplicity, assuming shape is correctly provided or inferred.
                        var ndArray = np.array(floatArray).reshape(schemaFeature.Shape.Select(s => (int)s).ToArray()); // Ensure shape matches
                        feedDict[i++] = new FeedItem(inputTensor, ndArray);
                    }
                    // Add more type conversions (int[], double[], etc.) and shape handling
                    else
                    {
                        _logger.LogError("Unsupported input type for feature '{FeatureName}': {FeatureType}", schemaFeature.Name, featureValue.GetType().Name);
                        throw new NotSupportedException($"Input type {featureValue.GetType().Name} for feature '{schemaFeature.Name}' is not supported.");
                    }
                }

                // Fetch operations also need to be defined, e.g., from _modelDetails.OutputSchema
                var fetchOps = _modelDetails.OutputSchema.Features
                                .Select(f => _graph.OperationByName(f.TensorName ?? f.Name)) // Or _graph.get_tensor_by_name(...)
                                .Where(op => op != null)
                                .ToArray();

                if(fetchOps.Length != _modelDetails.OutputSchema.Features.Count)
                {
                    _logger.LogError("One or more output tensors defined in schema could not be found in the graph.");
                    throw new InvalidOperationException("Mismatch between output schema and graph output tensors.");
                }

                var results = _session.run(fetchOps, feed_dict: feedDict.ToDictionary(fd => fd.Key, fd => fd.Value)); // TensorFlow.NET API might vary for feed_dict

                var outputData = new Dictionary<string, object>();
                for(int j=0; j < results.Length; j++)
                {
                    var outputSchemaFeature = _modelDetails.OutputSchema.Features[j];
                    var resultNdArray = results[j]; // This is an NDArray

                    // Convert NDArray to a more usable format, e.g., float[], object[,]
                    // This depends on the structure of the NDArray (shape, dtype)
                    // For simplicity, converting to a flat array or multidimensional array of a common type.
                    if (resultNdArray.dtype == TF_DataType.TF_FLOAT)
                    {
                        outputData[outputSchemaFeature.Name] = resultNdArray.ToArray<float>();
                    }
                    else if (resultNdArray.dtype == TF_DataType.TF_INT32)
                    {
                        outputData[outputSchemaFeature.Name] = resultNdArray.ToArray<int>();
                    }
                    // Add other dtype conversions
                    else
                    {
                         outputData[outputSchemaFeature.Name] = resultNdArray.numpy(); // Or some other generic representation
                        _logger.LogWarning("Unhandled TensorFlow output dtype for {OutputName}: {Dtype}", outputSchemaFeature.Name, resultNdArray.dtype);
                    }
                }

                _logger.LogDebug("TensorFlow model {ModelName} execution completed.", _modelDetails.Name);
                return Task.FromResult(new ModelOutput { Outputs = outputData, RawOutput = results });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing TensorFlow model {ModelName}", _modelDetails.Name);
                throw;
            }
        }


        public void Dispose()
        {
            _session?.close();
            _session?.Dispose();
            _graph?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}