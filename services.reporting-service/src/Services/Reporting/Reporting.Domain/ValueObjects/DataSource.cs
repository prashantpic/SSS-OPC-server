namespace Reporting.Domain.ValueObjects;

/// <summary>
/// Represents a data source for a report template.
/// </summary>
/// <param name="Name">The display name of the data source section in the report.</param>
/// <param name="Type">The type of data source (e.g., "HistoricalTrend", "AIAnomaly").</param>
/// <param name="Parameters">A dictionary of parameters needed to query this data source.</param>
public record DataSource(
    string Name,
    string Type,
    Dictionary<string, string> Parameters);