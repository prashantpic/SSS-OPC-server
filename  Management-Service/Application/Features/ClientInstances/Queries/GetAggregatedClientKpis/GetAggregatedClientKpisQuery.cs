using MediatR;
using ManagementService.Api.V1.DTOs; // For AggregatedKpisDto
using ManagementService.Domain.Aggregates.ClientInstanceAggregate.ValueObjects; // For ClientKpiFilter

namespace ManagementService.Application.Features.ClientInstances.Queries.GetAggregatedClientKpis;

// MediatR query representing a request to fetch aggregated Key Performance Indicators for clients.
public record GetAggregatedClientKpisQuery(
    ClientKpiFilter? FilterCriteria
) : IRequest<AggregatedKpisDto>;