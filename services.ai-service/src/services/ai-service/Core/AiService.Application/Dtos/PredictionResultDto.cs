namespace AiService.Application.Dtos;

/// <summary>
/// Data Transfer Object representing the result of a prediction operation.
/// </summary>
/// <param name="Prediction">The primary prediction value from the model.</param>
/// <param name="Metadata">Additional metadata or outputs from the prediction, such as feature importance.</param>
public record PredictionResultDto(double Prediction, Dictionary<string, object> Metadata);