using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Opc.System.Services.AI.Application.Interfaces;
using Opc.System.Services.AI.Application.Interfaces.Models;
using System.Collections.Concurrent;

// NOTE: The following interface and models are defined here to satisfy dependencies
// without creating unlisted files. Ideally, each would be in its own file in the Application layer.
namespace Opc.System.Services.AI.Application.Interfaces
{
    using Opc.System.Services.AI.Application.Interfaces.Models;
    
    /// <summary>
    /// Defines the contract for a service that can execute a machine learning model.
    /// </summary>
    public interface IModelRunner
    {
        /// <summary>
        /// Runs inference on a specified model with the given input data.
        /// </summary>
        Task<ModelOutputData> RunPredictionAsync(Guid modelId, string version, ModelInputData input, CancellationToken cancellationToken = default);
    }
}

namespace Opc.System.Services.AI.Application.Interfaces.Models
{
    /// <summary>
    /// Represents the input data for a model inference request.
    /// The dictionary holds named inputs corresponding to the model's input nodes.
    /// </summary>
    public record ModelInputData(IReadOnlyDictionary<string, object> Inputs);

    /// <summary>
    /// Represents the output data from a model inference execution.
    /// The dictionary holds named outputs corresponding to the model's output nodes.
    /// </summary>
    public record ModelOutputData(IReadOnlyDictionary<string, object> Outputs);
}
// End of embedded interface definitions.

namespace Opc.System.Services.AI.Infrastructure.Services
{
    /// <summary>
    /// A service responsible for executing ONNX models using the ONNX Runtime.
    /// </summary>
    public class OnnxModelRunner : IModelRunner, IDisposable
    {
        private readonly IModelStore _modelStore;
        // Caching inference sessions is crucial for performance, as session creation is expensive.
        private static readonly ConcurrentDictionary<string, InferenceSession> _sessionCache = new();

        public OnnxModelRunner(IModelStore modelStore)
        {
            _modelStore = modelStore;
        }

        /// <inheritdoc />
        public async Task<ModelOutputData> RunPredictionAsync(Guid modelId, string version, ModelInputData input, CancellationToken cancellationToken = default)
        {
            var session = await GetOrCreateInferenceSessionAsync(modelId, version, cancellationToken);
            var inputTensors = CreateInputTensors(input, session);

            using var results = session.Run(inputTensors);
            
            var outputs = new Dictionary<string, object>();
            foreach (var result in results)
            {
                outputs[result.Name] = ConvertOnnxValueToObject(result.Value);
            }

            return new ModelOutputData(outputs);
        }
        
        private async Task<InferenceSession> GetOrCreateInferenceSessionAsync(Guid modelId, string version, CancellationToken cancellationToken)
        {
            string cacheKey = $"{modelId}:{version}";
            if (!_sessionCache.TryGetValue(cacheKey, out var session))
            {
                using var modelStream = await _modelStore.GetModelStreamAsync(modelId.ToString(), version, cancellationToken);
                using var memoryStream = new MemoryStream();
                await modelStream.CopyToAsync(memoryStream, cancellationToken);
                var modelBytes = memoryStream.ToArray();

                session = new InferenceSession(modelBytes);
                _sessionCache.TryAdd(cacheKey, session);
            }
            return session;
        }

        private static List<NamedOnnxValue> CreateInputTensors(ModelInputData input, InferenceSession session)
        {
            var inputTensors = new List<NamedOnnxValue>();
            foreach (var inputPair in input.Inputs)
            {
                var inputMeta = session.InputMetadata[inputPair.Key];
                var tensor = CreateTensor(inputPair.Value, inputMeta.ElementType);
                inputTensors.Add(NamedOnnxValue.CreateFromTensor(inputPair.Key, tensor));
            }
            return inputTensors;
        }
        
        private static Tensor<T> ToTensor<T>(object data)
        {
            var typedData = (IEnumerable<T>)data;
            return new DenseTensor<T>(typedData.ToArray(), new[] { 1, typedData.Count() });
        }
        
        private static OrtValue CreateTensor(object data, OnnxValueType elementType)
        {
            // This is a simplified conversion. A robust implementation would handle various shapes and types.
            if(data is float[] floatArray)
            {
                 return OrtValue.CreateTensorValueFromMemory(floatArray, new long[] {1, floatArray.Length});
            }
            if(data is long[] longArray)
            {
                return OrtValue.CreateTensorValueFromMemory(longArray, new long[] {1, longArray.Length});
            }

            throw new NotSupportedException($"Data type {data.GetType()} is not supported for tensor creation.");
        }

        private object ConvertOnnxValueToObject(OrtValue value)
        {
            if (value.IsTensor)
            {
                var tensor = value.AsTensor<object>();
                // This is a simplification. The actual conversion would depend on tensor.ElementType
                // and the shape of the tensor. For a single value output it might be tensor.GetValue(0)
                // For an array it could be tensor.ToArray().
                if (tensor.ElementType == typeof(float)) return value.AsTensor<float>().ToArray();
                if (tensor.ElementType == typeof(long)) return value.AsTensor<long>().ToArray();
                if (tensor.ElementType == typeof(bool)) return value.AsTensor<bool>().ToArray();
                if (tensor.ElementType == typeof(string)) return value.AsTensor<string>().ToArray();
            }
            throw new NotSupportedException($"OrtValue type {value.OnnxType} is not supported for conversion.");
        }

        public void Dispose()
        {
            foreach (var session in _sessionCache.Values)
            {
                session.Dispose();
            }
            _sessionCache.Clear();
            GC.SuppressFinalize(this);
        }
    }
}