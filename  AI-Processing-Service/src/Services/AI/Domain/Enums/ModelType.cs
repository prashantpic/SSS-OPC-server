namespace AIService.Domain.Enums
{
    /// <summary>
    /// Defines the types of AI models supported by the service.
    /// (REQ-7-001, REQ-7-008)
    /// </summary>
    public enum ModelType
    {
        /// <summary>
        /// Model for predictive maintenance tasks.
        /// </summary>
        PredictiveMaintenance,

        /// <summary>
        /// Model for anomaly detection tasks.
        /// </summary>
        AnomalyDetection,

        /// <summary>
        /// Model for classifying user intent in Natural Language Queries.
        /// </summary>
        NlpIntentClassifier,

        /// <summary>
        /// Model for Named Entity Recognition in Natural Language Queries.
        /// </summary>
        NlpEntityRecognizer,

        /// <summary>
        /// Model for computer vision tasks, such as image classification or object detection.
        /// </summary>
        ComputerVision,

        /// <summary>
        /// A general-purpose or custom model type not fitting other categories.
        /// </summary>
        Generic
    }
}