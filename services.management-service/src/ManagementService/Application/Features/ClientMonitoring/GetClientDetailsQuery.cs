using ManagementService.Domain.ValueObjects;
using MediatR;

namespace ManagementService.Application.Features.ClientMonitoring;

// --- DTOs required for the query response ---
public record TagConfigDto(string TagName, int ScanRate);
public record ClientConfigurationDto(int PollingIntervalSeconds, IReadOnlyList<TagConfigDto> TagConfigurations);
public record HealthStatusDto(bool IsConnected, double DataThroughput, double CpuUsagePercent);

public record ClientDetailsDto(
    Guid Id,
    string Name,
    string Site,
    ClientConfigurationDto Configuration,
    HealthStatusDto HealthStatus,
    DateTimeOffset LastSeen
);

/// <summary>
/// A CQRS query to retrieve details for one OpcClientInstance.
/// </summary>
/// <param name="ClientId">The unique identifier of the client to retrieve.</param>
public record GetClientDetailsQuery(Guid ClientId) : IRequest<ClientDetailsDto?>;