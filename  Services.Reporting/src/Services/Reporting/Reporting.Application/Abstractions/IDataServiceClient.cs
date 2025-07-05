using Opc.System.Services.Reporting.Application.Models;

namespace Opc.System.Services.Reporting.Application.Abstractions;

/// <summary>
/// Defines the contract for a client that fetches operational data from the central Data Service.
/// To abstract the communication with the Data Service, decoupling the application logic from the specific gRPC/REST client implementation.
/// </summary>
public interface IDataServiceClient
{
    /// <summary>
    /// Fetches historical data based on a specified query.
    /// </summary>
    /// <param name="query">The query parameters for fetching historical data.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A collection of data points.</returns>
    Task<IEnumerable<DataPoint>> GetHistoricalDataAsync(HistoricalDataQuery query, CancellationToken cancellationToken);
}

// NOTE: The DTOs below would typically be in a separate Models folder or a shared project.
// They are included here for compilation and clarity.

namespace Opc.System.Services.Reporting.Application.Models
{
    /// <summary>
    /// Represents a query for historical data.
    /// </summary>
    public record HistoricalDataQuery(IEnumerable<string> Tags, DateTimeOffset StartTime, DateTimeOffset EndTime, string Aggregation);

    /// <summary>
    /// Represents a single time-series data point.
    /// </summary>
    public record DataPoint(string Tag, DateTimeOffset Timestamp, object Value);
}