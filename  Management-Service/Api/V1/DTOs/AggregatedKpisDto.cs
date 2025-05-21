using System;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents aggregated KPIs (e.g., total clients, connected clients, average health score, error rates) for a set of clients.
/// </summary>
public record AggregatedKpisDto(
    int TotalClients,
    int ConnectedClients,
    double AverageHealthScore,
    double ErrorRatePercentage
);