using MediatR;
using ManagementService.Application.Abstractions.Services;
using ManagementService.Application.Abstractions.Services.Dto;
using ManagementService.Api.V1.DTOs;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate.ValueObjects;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ManagementService.Application.Features.ClientInstances.Queries.GetAggregatedClientKpis;

public class GetAggregatedClientKpisQueryHandler : IRequestHandler<GetAggregatedClientKpisQuery, AggregatedKpisDto>
{
    private readonly IDataServiceApiClient _dataServiceApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAggregatedClientKpisQueryHandler> _logger;

    public GetAggregatedClientKpisQueryHandler(
        IDataServiceApiClient dataServiceApiClient,
        IMapper mapper,
        ILogger<GetAggregatedClientKpisQueryHandler> logger)
    {
        _dataServiceApiClient = dataServiceApiClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AggregatedKpisDto> Handle(GetAggregatedClientKpisQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetAggregatedClientKpisQuery with filter: {@Filter}", request.FilterCriteria);

        // Map the application-layer filter (domain object) to the DTO expected by the DataServiceApiClient if necessary.
        // For this example, let's assume ClientKpiFilter is directly usable or easily mappable.
        var dataServiceFilter = request.FilterCriteria ?? ClientKpiFilter.Default;

        try
        {
            var aggregatedDataFromService = await _dataServiceApiClient.GetAggregatedKpisAsync(dataServiceFilter, cancellationToken);

            _logger.LogInformation("Received aggregated KPI data from DataService: {@AggregatedData}", aggregatedDataFromService);

            // Map the result from the DataService DTO (AggregatedKpiResult) to the API DTO (AggregatedKpisDto)
            var resultDto = _mapper.Map<AggregatedKpisDto>(aggregatedDataFromService);

            return resultDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving or mapping aggregated KPIs from DataService for filter {@Filter}", dataServiceFilter);
            // Depending on the desired error handling strategy, you might:
            // 1. Re-throw the exception (to be caught by middleware).
            // 2. Return a default/empty DTO.
            // 3. Wrap the result in a custom Result object indicating success/failure.
            // For now, re-throwing.
            throw;
        }
    }
}