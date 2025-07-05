namespace Reporting.Application.Contracts.Services;

/// <summary>
/// DTO for an anomaly insight retrieved from the AI service.
/// </summary>
public record AnomalyInsightDto(
    DateTime Timestamp,
    string Tag,
    double Value,
    double AnomalyScore,
    string Description
);

/// <summary>
/// DTO for a maintenance prediction retrieved from the AI service.
/// </summary>
public record MaintenanceInsightDto(
    string AssetId,
    string Prediction,
    double Confidence,
    DateTime HorizonDate
);

/// <summary>
/// DTO for a historical data point retrieved from the Data service.
/// </summary>
public record HistoricalDataDto(
    DateTime Timestamp,
    string Tag,
    object Value
);