using ManagementService.Api.V1.DTOs; // For AggregatedKpisDto
using ManagementService.Application.Abstractions.Services.Dto;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate.ValueObjects;


namespace ManagementService.Application.Abstractions.Services;

public interface IDataServiceApiClient
{
    Task<bool> StoreClientKpiDataAsync(ClientKpiData data, CancellationToken cancellationToken);
    Task<AggregatedKpisDto> GetAggregatedKpisAsync(ClientKpiFilterDto? queryParams, CancellationToken cancellationToken);
}

// Defined here as per SDS structure for this file
namespace ManagementService.Application.Abstractions.Services.Dto
{
    public record ClientKpiData(
        Guid ClientInstanceId,
        DateTimeOffset Timestamp,
        double HealthScore,
        double ErrorRate,
        string Status
    );

    // AggregatedKpiResult is effectively AggregatedKpisDto from Api.V1.DTOs
    // So we can use that directly or define a separate internal DTO if structures differ.
    // For simplicity, let's assume the method directly returns the API DTO or a compatible structure.
}