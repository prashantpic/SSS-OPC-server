namespace Reporting.Application.Contracts.Services;

/// <summary>
/// Interface defining the contract for communicating with the external Data Service.
/// </summary>
public interface IDataServiceClient
{
    /// <summary>
    /// Retrieves historical data for a specific tag and time range.
    /// </summary>
    Task<List<HistoricalDataDto>> GetHistoricalDataAsync(string tag, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}