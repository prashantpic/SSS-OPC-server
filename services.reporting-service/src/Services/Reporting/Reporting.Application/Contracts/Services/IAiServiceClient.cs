namespace Reporting.Application.Contracts.Services;

/// <summary>
/// Interface defining the contract for communicating with the external AI Service.
/// </summary>
public interface IAiServiceClient
{
    /// <summary>
    /// Retrieves detected anomalies for a given time range.
    /// </summary>
    Task<List<AnomalyInsightDto>> GetAnomaliesForReportAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves predictive maintenance forecasts for a list of assets.
    /// </summary>
    Task<List<MaintenanceInsightDto>> GetMaintenancePredictionsAsync(List<string> assetIds, CancellationToken cancellationToken = default);
}