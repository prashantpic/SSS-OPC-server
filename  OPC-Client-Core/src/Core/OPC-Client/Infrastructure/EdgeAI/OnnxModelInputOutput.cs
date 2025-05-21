namespace OPC.Client.Core.Infrastructure.EdgeAI
{
    using System.Collections.Generic;

    /// <summary>
    /// Specifies the data contract (input features and output predictions/results)
    /// for AI models run at the edge using the OnnxRuntimeHost.
    /// Implements REQ-7-001, REQ-8-001.
    /// </summary>
    public class OnnxModelInput
    {
        /// <summary>
        /// A dictionary where keys are the input names expected by the ONNX model,
        /// and values are the corresponding input data (e.g., float arrays, tensors).
        /// The specific types of values depend on the model's requirements.
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        public OnnxModelInput()
        {
            Inputs = new Dictionary<string, object>();
        }

        public OnnxModelInput(Dictionary<string, object> inputs)
        {
            Inputs = inputs ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Represents the output data from an ONNX model execution.
    /// </summary>
    public class OnnxModelOutput
    {
        /// <summary>
        /// A dictionary where keys are the output names produced by the ONNX model,
        /// and values are the corresponding output data (e.g., predictions, classifications).
        /// The specific types of values depend on the model's outputs.
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; }

        public OnnxModelOutput()
        {
            Outputs = new Dictionary<string, object>();
        }

        public OnnxModelOutput(Dictionary<string, object> outputs)
        {
            Outputs = outputs ?? new Dictionary<string, object>();
        }
    }
}