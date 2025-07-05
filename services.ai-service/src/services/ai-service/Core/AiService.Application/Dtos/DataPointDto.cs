namespace AiService.Application.Dtos;

/// <summary>
/// Data Transfer Object representing a single point of time-series data.
/// </summary>
/// <param name="Timestamp">The timestamp of the data point.</param>
/// <param name="Tag">The identifier for the data source or tag.</param>
/// <param name="Value">The value of the data point.</param>
public record DataPointDto(DateTime Timestamp, string Tag, double Value);