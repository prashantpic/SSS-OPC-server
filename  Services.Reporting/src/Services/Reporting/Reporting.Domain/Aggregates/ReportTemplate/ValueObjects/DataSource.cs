namespace Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate.ValueObjects;

/// <summary>
/// Encapsulates the configuration for a source of data to be included in a report.
/// A value object representing a single data source for a report, such as a set of OPC tags, a historical data query, or an AI insight.
/// </summary>
public record DataSource
{
    /// <summary>
    /// Defines the type of data source (e.g., "Historical", "Realtime", "AIInsight").
    /// </summary>
    public string SourceType { get; init; }

    /// <summary>
    /// Holds the specific query details, like tag names or time ranges.
    /// </summary>
    public Dictionary<string, string> Parameters { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSource"/> record.
    /// </summary>
    /// <param name="sourceType">The type of the data source.</param>
    /// <param name="parameters">The parameters for querying the data source.</param>
    public DataSource(string sourceType, Dictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(sourceType))
            throw new ArgumentException("Source type must be provided.", nameof(sourceType));

        SourceType = sourceType;
        Parameters = parameters ?? new Dictionary<string, string>();
    }
}