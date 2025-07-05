namespace ManagementService.Domain.ValueObjects;

/// <summary>
/// A value object representing the health status reported by a client,
/// including KPIs like connection status and throughput. Immutable.
/// </summary>
/// <param name="IsConnected">Indicates if the OPC client is connected to its target server.</param>
/// <param name="DataThroughput">The rate of data points per second being processed.</param>
/// <param name="CpuUsagePercent">The CPU usage of the client process.</param>
public record HealthStatus(bool IsConnected, double DataThroughput, double CpuUsagePercent)
{
    /// <summary>
    /// Represents the initial, unknown state of a new client.
    /// </summary>
    public static HealthStatus Initial => new(false, 0.0, 0.0);
}