namespace AiService.Application.Dtos;

/// <summary>
/// Data Transfer Object representing a detected anomaly.
/// </summary>
/// <param name="Timestamp">The timestamp when the anomaly occurred.</param>
/// <param name="Tag">The identifier of the data stream where the anomaly was detected.</param>
/// <param name="Value">The value that was considered anomalous.</param>
/// <param name="Score">A score indicating the severity or confidence of the anomaly.</param>
/// <param name="IsAnomaly">A boolean flag confirming if this point is an anomaly.</param>
public record AnomalyDto(DateTime Timestamp, string Tag, double Value, double Score, bool IsAnomaly);