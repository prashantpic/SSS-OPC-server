using ManagementService.Application.Contracts.Persistence;
using MediatR;

namespace ManagementService.Application.Features.ClientMonitoring;

/// <summary>
/// The handler for the GetClientDetailsQuery. It retrieves the client instance
/// from the repository and maps it to a response DTO.
/// </summary>
public class GetClientDetailsQueryHandler : IRequestHandler<GetClientDetailsQuery, ClientDetailsDto?>
{
    private readonly IOpcClientInstanceRepository _clientRepository;

    public GetClientDetailsQueryHandler(IOpcClientInstanceRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public async Task<ClientDetailsDto?> Handle(GetClientDetailsQuery request, CancellationToken cancellationToken)
    {
        var clientInstance = await _clientRepository.GetByIdAsync(request.ClientId);

        if (clientInstance == null)
        {
            return null; // The controller will handle turning this into a 404 Not Found.
        }

        // Manual mapping from Domain object to DTO. AutoMapper could be used here.
        var clientDetailsDto = new ClientDetailsDto(
            clientInstance.Id,
            clientInstance.Name,
            clientInstance.Site,
            new ClientConfigurationDto(
                clientInstance.Configuration.PollingIntervalSeconds,
                clientInstance.Configuration.TagConfigurations
                    .Select(t => new TagConfigDto(t.TagName, t.ScanRate))
                    .ToList()
                    .AsReadOnly()
            ),
            new HealthStatusDto(
                clientInstance.HealthStatus.IsConnected,
                clientInstance.HealthStatus.DataThroughput,
                clientInstance.HealthStatus.CpuUsagePercent
            ),
            clientInstance.LastSeen
        );

        return clientDetailsDto;
    }
}