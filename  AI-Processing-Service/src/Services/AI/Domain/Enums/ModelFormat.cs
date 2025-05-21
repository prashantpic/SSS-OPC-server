namespace AIService.Domain.Enums
{
    /// <summary>
    /// Defines the file formats of AI models supported by the service.
    /// (REQ-7-001, REQ-7-003, REQ-8-001)
    /// </summary>
    public enum ModelFormat
    {
        /// <summary>
        /// Open Neural Network Exchange format.
        /// </summary>
        ONNX,

        /// <summary>
        /// TensorFlow Lite format, optimized for mobile and edge devices.
        /// </summary>
        TensorFlowLite,

        /// <summary>
        /// Standard TensorFlow SavedModel format.
        /// </summary>
        TensorFlowSavedModel,
        
        /// <summary>
        /// ML.NET model format (typically a .zip file).
        /// </summary>
        MLNetZip,

        /// <summary>
        /// PyTorch Model format (e.g. .pt or .pth files)
        /// Often converted to ONNX for deployment.
        /// </summary>
        PyTorch,
        
        /// <summary>
        /// Other or custom model format.
        /// </summary>
        Other
    }
}