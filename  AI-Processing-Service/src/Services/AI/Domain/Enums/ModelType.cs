namespace AIService.Domain.Enums
{
    /// <summary>
    /// Defines the types of AI models supported, e.g.,
    /// PredictiveMaintenance, AnomalyDetection, NlpIntentClassifier.
    /// </summary>
    public enum ModelType
    {
        Undefined = 0,
        PredictiveMaintenance = 1,
        AnomalyDetection = 2,
        NlpIntentClassifier = 3,
        NlpEntityRecognition = 4,
        ImageClassification = 5,
        ObjectDetection = 6,
        Generic = 7 // For models not fitting specific categories but usable by the engine
    }
}