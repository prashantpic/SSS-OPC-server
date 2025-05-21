namespace AIService.Domain.Enums
{
    /// <summary>
    /// Defines the file formats of AI models, e.g., ONNX, TensorFlowLite, MLNetZip.
    /// </summary>
    public enum ModelFormat
    {
        Undefined = 0,
        ONNX = 1,
        TensorFlowLite = 2,
        MLNetZip = 3,
        TensorFlowSavedModel = 4, // For TensorFlow (non-Lite) saved models
        PyTorch = 5, // Placeholder, requires specific handling
        SpaCy = 6 // For NLP models from spaCy
    }
}