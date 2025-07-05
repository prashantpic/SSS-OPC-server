namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Represents the structured output from an NLP service after interpreting a user's query.
/// </summary>
public class StructuredNlpResult
{
    /// <summary>
    /// The primary intent recognized in the query (e.g., "get_average", "get_timeseries").
    /// </summary>
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// A dictionary of entities extracted from the query.
    /// Keys are entity types (e.g., "tag_name", "time_range_start"), values are the extracted text.
    /// </summary>
    public Dictionary<string, string> Entities { get; set; } = new();

    /// <summary>
    /// The confidence score of the intent recognition, from 0.0 to 1.0.
    /// </summary>
    public double Confidence { get; set; }
}