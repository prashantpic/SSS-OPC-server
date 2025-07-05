namespace AiService.Domain.Enums;

/// <summary>
/// Enumeration for the different types of AI models supported by the service.
/// To provide a strongly-typed classification for AI models.
/// </summary>
public enum ModelType
{
    /// <summary>
    /// A model used for predicting future maintenance needs of equipment.
    /// </summary>
    PredictiveMaintenance,

    /// <summary>
    /// A model used for detecting anomalous patterns in data streams.
    /// </summary>
    AnomalyDetection
}