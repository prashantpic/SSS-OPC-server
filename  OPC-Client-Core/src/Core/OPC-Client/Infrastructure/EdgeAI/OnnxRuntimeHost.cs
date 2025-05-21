using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent; // For caching sessions

namespace OPC.Client.Core.Infrastructure.EdgeAI
{
    /// <summary>
    /// Represents the input for an ONNX model.
    /// This is a simplified structure; actual models might require specific tensor shapes and types.
    /// </summary>
    public class OnnxModelInput
    {
        /// <summary>
        /// Dictionary of input names and their corresponding tensor data.
        /// The value should be compatible with DenseTensor or other ONNX tensor types.
        /// Example: For a model expecting a float array named "input", value would be float[].
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the output from an ONNX model.
    /// </summary>
    public class OnnxModelOutput
    {
        /// <summary>
        /// Dictionary of output names and their corresponding tensor data.
        /// The value will typically be an array or scalar of the model's output type.
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines the contract for running ONNX models.
    /// </summary>
    public interface IOnnxModelRunner
    {
        /// <summary>
        /// Loads an ONNX model (if not already cached) and runs inference.
        /// </summary>
        /// <param name="modelPath">The file path to the ONNX model.</param>
        /// <param name="modelInput">The input data for the model.</param>
        /// <returns>The output from the model inference.</returns>
        Task<OnnxModelOutput> RunAsync(string modelPath, OnnxModelInput modelInput);

        /// <summary>
        /// Preloads a model into the cache.
        /// </summary>
        /// <param name="modelPath">The file path to the ONNX model.</param>
        Task PreloadModelAsync(string modelPath);
    }


    /// <summary>
    /// Wrapper for ONNX Runtime to load and execute AI models (in ONNX format) at the edge.
    /// Provides functionality to load ONNX models and run inference using the ONNX Runtime library.
    /// REQ-7-001, REQ-8-001
    /// </summary>
    public class OnnxRuntimeHost : IOnnxModelRunner, IDisposable
    {
        private readonly ILogger<OnnxRuntimeHost> _logger;
        private readonly ConcurrentDictionary<string, InferenceSession> _sessionCache = new ConcurrentDictionary<string, InferenceSession>();
        private bool _disposed = false;

        public OnnxRuntimeHost(ILogger<OnnxRuntimeHost> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _logger.LogInformation("ONNX Runtime Host initialized. Ensure ONNX Runtime native libraries are deployed correctly for the target platform.");
        }

        public async Task PreloadModelAsync(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
            {
                throw new ArgumentException("Model path cannot be null or empty.", nameof(modelPath));
            }

            if (!_sessionCache.ContainsKey(modelPath))
            {
                await LoadModelAsync(modelPath);
            }
        }

        private async Task<InferenceSession> LoadModelAsync(string modelPath)
        {
            _logger.LogInformation("Loading ONNX model from path: {ModelPath}", modelPath);
            try
            {
                // SessionOptions can be configured for performance (e.g., execution providers like CUDA, TensorRT)
                var sessionOptions = new SessionOptions();
                // Example: Enable CPU execution provider (default)
                // sessionOptions.AppendExecutionProvider_CPU(0);
                // Example: Enable CUDA if available
                // if (OrtEnv.IsCudaAvailable()) sessionOptions.AppendExecutionProvider_CUDA(0);

                var session = await Task.Run(() => new InferenceSession(modelPath, sessionOptions));
                _sessionCache.TryAdd(modelPath, session);
                _logger.LogInformation("ONNX model loaded and cached successfully: {ModelPath}", modelPath);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load ONNX model from path: {ModelPath}", modelPath);
                throw new OnnxModelLoadException($"Failed to load ONNX model: {modelPath}", ex);
            }
        }


        public async Task<OnnxModelOutput> RunAsync(string modelPath, OnnxModelInput modelInput)
        {
            if (string.IsNullOrWhiteSpace(modelPath))
                throw new ArgumentException("Model path cannot be null or empty.", nameof(modelPath));
            if (modelInput == null || modelInput.Inputs == null || !modelInput.Inputs.Any())
                throw new ArgumentNullException(nameof(modelInput), "Model input cannot be null or empty.");

            InferenceSession session;
            if (!_sessionCache.TryGetValue(modelPath, out session!))
            {
                session = await LoadModelAsync(modelPath);
            }

            _logger.LogDebug("Running inference for model: {ModelPath}", modelPath);

            var inputTensors = new List<NamedOnnxValue>();
            try
            {
                foreach (var inputPair in modelInput.Inputs)
                {
                    // This is a simplified tensor creation. Real-world usage requires knowing
                    // the exact data type and shape expected by the model's input nodes.
                    // The 'object' in modelInput.Inputs needs to be converted to a specific Tensor type.
                    NamedOnnxValue onnxValue;
                    if (inputPair.Value is float[] floatArray) // Example for float array input
                    {
                        // Assuming 1D tensor for simplicity. For multi-dimensional, provide dims.
                        var tensor = new DenseTensor<float>(floatArray, new int[] { floatArray.Length });
                        onnxValue = NamedOnnxValue.CreateFromTensor(inputPair.Key, tensor);
                    }
                    else if (inputPair.Value is long[] longArray) // Example for long array input
                    {
                        var tensor = new DenseTensor<long>(longArray, new int[] { longArray.Length });
                        onnxValue = NamedOnnxValue.CreateFromTensor(inputPair.Key, tensor);
                    }
                    // Add more type checks and tensor conversions as needed based on expected model inputs
                    // e.g., for images, text, multi-dimensional arrays.
                    else
                    {
                        _logger.LogError("Unsupported input type for ONNX model: {InputType} for input name {InputName}", inputPair.Value?.GetType().Name, inputPair.Key);
                        throw new NotSupportedException($"Input type {inputPair.Value?.GetType().Name} for input '{inputPair.Key}' is not handled.");
                    }
                    inputTensors.Add(onnxValue);
                }

                // Specify output node names if known, otherwise ONNX runtime returns all outputs.
                // List<string> outputNames = session.OutputMetadata.Keys.ToList(); // Get all output names

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = await Task.Run(() => session.Run(inputTensors));

                var modelOutput = new OnnxModelOutput();
                foreach (var resultValue in results)
                {
                    // Convert ONNX output tensor back to a .NET friendly type.
                    // This is also simplified. ResultValue.ValueAsTensor gives access to tensor data.
                    if (resultValue.Value is OrtValue ortValue) // Newer ONNX Runtime versions
                    {
                         if (ortValue.IsTensor)
                         {
                             // Example: If output is a float tensor
                             if (ortValue.GetTensorElementType() == TensorElementType.Float)
                             {
                                 modelOutput.Outputs[resultValue.Name] = ortValue.GetTensorDataAsSpan<float>().ToArray();
                             }
                             // Add more output type handling
                             else
                             {
                                 _logger.LogWarning("Unhandled ONNX output tensor type: {ElementType} for output {OutputName}", ortValue.GetTensorElementType(), resultValue.Name);
                                 modelOutput.Outputs[resultValue.Name] = ortValue.Value; // Store OrtValue directly if type unknown
                             }
                         } else {
                             modelOutput.Outputs[resultValue.Name] = ortValue.Value; // For non-tensor outputs
                         }
                    }
                    else // Older ONNX Runtime might return DenseTensor directly
                    {
                         modelOutput.Outputs[resultValue.Name] = resultValue.Value; // Or process resultValue.AsTensor()
                    }


                }
                _logger.LogDebug("Inference completed successfully for model: {ModelPath}", modelPath);
                return modelOutput;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ONNX model inference for path: {ModelPath}", modelPath);
                throw new OnnxInferenceException($"Error during ONNX model inference: {modelPath}", ex);
            }
            finally
            {
                // Dispose input tensors if they are IDisposable and created explicitly
                foreach (var tensor in inputTensors)
                {
                    (tensor as IDisposable)?.Dispose();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var sessionEntry in _sessionCache)
                {
                    sessionEntry.Value?.Dispose();
                }
                _sessionCache.Clear();
            }
            _disposed = true;
        }
    }

    public class OnnxModelLoadException : Exception
    {
        public OnnxModelLoadException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class OnnxInferenceException : Exception
    {
        public OnnxInferenceException(string message, Exception innerException) : base(message, innerException) { }
    }
}