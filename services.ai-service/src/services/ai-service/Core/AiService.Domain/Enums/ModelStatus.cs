namespace AiService.Domain.Enums;

/// <summary>
/// Represents the deployment status lifecycle of an AI model.
/// </summary>
public enum ModelStatus
{
    /// <summary>
    /// The model has been registered in the system but is not yet deployed for inference.
    /// </summary>
    Registered,

    /// <summary>
    /// The model is active and available for making predictions.
    /// </summary>
    Deployed,

    /// <summary>
    /// The model has been superseded by a new version or is no longer in use.
    /// </summary>
    Retired
}