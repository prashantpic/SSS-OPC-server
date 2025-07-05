using Opc.System.Services.Reporting.Application.Models;

namespace Opc.System.Services.Reporting.Application.Abstractions;

/// <summary>
/// Defines the contract for a client that fetches analytical insights from the AI Service.
/// To abstract the communication with the AI Service, enabling the reporting application logic to request AI-driven insights without direct knowledge of the transport mechanism.
/// </summary>
public interface IAiServiceClient
{
    /// <summary>
    /// Fetches anomaly insights within a given time range.
    /// </summary>
    /// <param name="timeRange">The time range for the query.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of anomaly insights.</returns>
    Task<IEnumerable<AnomalyInsight>> GetAnomaliesAsync(TimeRange timeRange, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches a summary of trends within a given time range.
    /// </summary>
    /// <param name="timeRange">The time range for the query.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A summary of trends.</returns>
    Task<TrendSummary> GetTrendInsightsAsync(TimeRange timeRange, CancellationToken cancellationToken);
}


// NOTE: The DTOs below would typically be in a separate Models folder or a shared project.
// They are included here for compilation and clarity.

namespace Opc.System.Services.Reporting.Application.Models
{
    /// <summary>
    /// Represents a time range.
    /// </summary>
    public record TimeRange(DateTimeOffset StartTime, DateTimeOffset EndTime);

    /// <summary>
    /// Represents an anomaly detected by the AI service.
    /// </summary>
    public record AnomalyInsight(string Tag, DateTimeOffset Timestamp, double ActualValue, double ExpectedValue, double Severity);

    /// <summary>
    /// Represents a summary of trends detected by the AI service.
    /// </summary>
    public record TrendSummary(string Description, IEnumerable<string> RelevantTags);
}