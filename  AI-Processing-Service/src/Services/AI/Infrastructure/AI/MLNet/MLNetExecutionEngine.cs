using AIService.Domain.Enums;
using AIService.Domain.Interfaces;
using AIService.Domain.Models;
using AIService.Infrastructure.AI.Common;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AIService.Infrastructure.AI.MLNet
{
    public class MLNetExecutionEngine : IModelExecutionEngine
    {
        private readonly ILogger<MLNetExecutionEngine> _logger;
        private readonly ModelFileLoader _modelFileLoader;
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private AiModel _modelDetails;
        private Type _predictionInputType;
        private Type _predictionOutputType;
        private MethodInfo _predictMethod;


        public MLNetExecutionEngine(ILogger<MLNetExecutionEngine> logger, ModelFileLoader modelFileLoader)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelFileLoader = modelFileLoader ?? throw new ArgumentNullException(nameof(modelFileLoader));
            _mlContext = new MLContext(seed: 0); // Seed for reproducibility
        }

        public ModelFormat SupportedFormat => ModelFormat.MLNetZip;

        public bool CanHandle(AiModel model)
        {
            return model?.ModelFormat == SupportedFormat;
        }

        public async Task LoadModelAsync(AiModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.ModelFormat != SupportedFormat)
                throw new ArgumentException($"Model format {model.ModelFormat} is not supported by {nameof(MLNetExecutionEngine)}.", nameof(model));

            _modelDetails = model;
            _logger.LogInformation("Loading ML.NET model: {ModelName} Version: {ModelVersion} from {StorageReference}", model.Name, model.Version, model.StorageReference);

            try
            {
                var modelStream = await _modelFileLoader.LoadModelFileAsync(model.StorageReference);
                if (modelStream == null || modelStream.Length == 0)
                {
                    throw new FileNotFoundException($"Model file could not be loaded from {model.StorageReference}");
                }

                // MLContext.Model.Load needs a seekable stream. If modelStream is not, copy to MemoryStream.
                MemoryStream seekableStream;
                if (modelStream.CanSeek)
                {
                    modelStream.Seek(0, SeekOrigin.Begin); // Ensure it's at the beginning
                    seekableStream = modelStream as MemoryStream ?? new MemoryStream(); // Avoid re-copy if already MemoryStream
                    if (seekableStream != modelStream) // if it was not a memory stream
                    {
                         await modelStream.CopyToAsync(seekableStream);
                         seekableStream.Seek(0, SeekOrigin.Begin);
                         await modelStream.DisposeAsync(); // Dispose original stream
                    }
                }
                else
                {
                    seekableStream = new MemoryStream();
                    await modelStream.CopyToAsync(seekableStream);
                    seekableStream.Seek(0, SeekOrigin.Begin);
                    await modelStream.DisposeAsync();
                }


                DataViewSchema modelSchema;
                _model = _mlContext.Model.Load(seekableStream, out modelSchema);
                await seekableStream.DisposeAsync();


                _logger.LogInformation("ML.NET model {ModelName} loaded successfully.", model.Name);

                // Dynamically determine input and output types based on AiModel.InputSchema and AiModel.OutputSchema
                // This is a complex part. ML.NET models usually have specific C# classes for input and output.
                // The schema might define these classes (e.g., class name, property names, types).
                // For now, this is a placeholder for dynamic type creation or lookup.
                // Option 1: Assume types are in a known assembly and can be loaded by name from schema.
                // Option 2: Dynamically compile types (more complex).
                // Option 3: Use a generic approach if ML.NET APIs allow it (e.g., creating DataView from ModelInput directly).
                // For this example, let's assume schema contains type names.

                if (string.IsNullOrWhiteSpace(_modelDetails.InputSchema?.ClassName) || string.IsNullOrWhiteSpace(_modelDetails.OutputSchema?.ClassName))
                {
                    _logger.LogError("InputSchema.ClassName or OutputSchema.ClassName is not defined for ML.NET model {ModelName}. Dynamic type resolution is required.", _modelDetails.Name);
                    throw new InvalidOperationException("ML.NET model schema does not define input/output class names.");
                }

                _predictionInputType = Type.GetType(_modelDetails.InputSchema.ClassName, throwOnError: true);
                _predictionOutputType = Type.GetType(_modelDetails.OutputSchema.ClassName, throwOnError: true);

                // Get the generic CreatePredictionEngine method
                var genericCreateEngineMethod = typeof(PredictionEngineExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "CreatePredictionEngine" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

                if (genericCreateEngineMethod == null)
                    throw new NotSupportedException("Could not find CreatePredictionEngine generic method.");

                var createEngineMethod = genericCreateEngineMethod.MakeGenericMethod(_predictionInputType, _predictionOutputType);
                var predictionEngine = createEngineMethod.Invoke(null, new object[] { _mlContext, _model });

                _predictMethod = predictionEngine.GetType().GetMethod("Predict", new[] { _predictionInputType });
                if (_predictMethod == null)
                    throw new NotSupportedException("Could not find Predict method on the prediction engine.");

                // Store the predictionEngine if preferred for reuse (though it's typically lightweight)
                // _predictionEngine = predictionEngine;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading ML.NET model {ModelName} from {StorageReference}", model.Name, model.StorageReference);
                throw;
            }
        }

        public Task<ModelOutput> ExecuteAsync(ModelInput input)
        {
            if (_model == null || _predictMethod == null || _predictionInputType == null)
            {
                _logger.LogError("ML.NET model or prediction components are not initialized. Load the model first.");
                throw new InvalidOperationException("Model not loaded or not configured correctly. Call LoadModelAsync first.");
            }

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            _logger.LogDebug("Executing ML.NET model {ModelName} with input.", _modelDetails.Name);

            try
            {
                // Create an instance of the dynamically determined input type
                var predictionInputInstance = Activator.CreateInstance(_predictionInputType);

                // Map features from ModelInput to properties of predictionInputInstance
                // This requires reflection and matching property names.
                // Assuming ModelInput.Features keys match property names in the _predictionInputType.
                foreach (var feature in input.Features)
                {
                    var propertyInfo = _predictionInputType.GetProperty(feature.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        // Convert feature.Value to propertyInfo.PropertyType
                        var convertedValue = Convert.ChangeType(feature.Value, propertyInfo.PropertyType);
                        propertyInfo.SetValue(predictionInputInstance, convertedValue);
                    }
                    else
                    {
                        _logger.LogWarning("Property {PropertyName} not found or not writable on ML.NET input type {InputTypeName} for model {ModelName}.",
                            feature.Key, _predictionInputType.Name, _modelDetails.Name);
                    }
                }

                // The prediction engine should be created per-thread or be thread-safe.
                // For simplicity, creating it here. For performance, it might be pooled or created once if thread-safe.
                // Re-creating prediction engine for thread safety per call.
                var genericCreateEngineMethod = typeof(PredictionEngineExtensions).GetMethod("CreatePredictionEngine", new[] { typeof(MLContext), typeof(ITransformer), typeof(DataViewSchema) });
                var createEngineMethod = genericCreateEngineMethod.MakeGenericMethod(_predictionInputType, _predictionOutputType);
                
                // We need to pass the model schema if the model was loaded with one.
                // However, the simpler overload `CreatePredictionEngine<TSrc, TDst>(MLContext, ITransformer)` is often used.
                // Let's use the one that doesn't require the schema explicitly on `CreatePredictionEngine` if _model.Schema is available.
                
                object predictionEngine;
                var createEngineMethodSimple = typeof(PredictionEngineExtensions)
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .First(m => m.Name == "CreatePredictionEngine" && m.IsGenericMethodDefinition && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(ITransformer))
                    .MakeGenericMethod(_predictionInputType, _predictionOutputType);

                predictionEngine = createEngineMethodSimple.Invoke(null, new object[] { _mlContext, _model });
                var predictMethodOnEngine = predictionEngine.GetType().GetMethod("Predict", new[] { _predictionInputType });


                var predictionResult = predictMethodOnEngine.Invoke(predictionEngine, new[] { predictionInputInstance });

                // Map properties from predictionResult (of _predictionOutputType) to ModelOutput.Outputs
                var outputData = new Dictionary<string, object>();
                var outputProperties = _predictionOutputType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in outputProperties)
                {
                    if (prop.CanRead)
                    {
                        outputData[prop.Name] = prop.GetValue(predictionResult);
                    }
                }

                _logger.LogDebug("ML.NET model {ModelName} execution completed.", _modelDetails.Name);
                return Task.FromResult(new ModelOutput { Outputs = outputData, RawOutput = predictionResult });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ML.NET model {ModelName}", _modelDetails.Name);
                throw;
            }
        }

        public void Dispose()
        {
            // MLContext and ITransformer might be disposable in some contexts, but typically not explicitly.
            // If _mlContext owns unmanaged resources, it should be disposed.
            // For this example, assuming they are managed or disposed via GC.
            GC.SuppressFinalize(this);
        }
    }
}