using ManagementService.Application.Contracts.Messaging;
using ManagementService.Application.Contracts.Persistence;
using ManagementService.Domain.ValueObjects;
using MediatR;

namespace ManagementService.Application.Features.BulkOperations;

// The command record to trigger a bulk configuration update.
public record ExecuteBulkConfigurationCommand(List<Guid> ClientIds, ClientConfiguration Configuration) : IRequest;

// The handler for executing a configuration update across multiple client instances.
public class ExecuteBulkConfigurationCommandHandler : IRequestHandler<ExecuteBulkConfigurationCommand>
{
    private readonly IOpcClientInstanceRepository _clientRepository;
    private readonly IConfigurationUpdatePublisher _configPublisher;
    private readonly ILogger<ExecuteBulkConfigurationCommandHandler> _logger;

    public ExecuteBulkConfigurationCommandHandler(
        IOpcClientInstanceRepository clientRepository,
        IConfigurationUpdatePublisher configPublisher,
        ILogger<ExecuteBulkConfigurationCommandHandler> logger)
    {
        _clientRepository = clientRepository;
        _configPublisher = configPublisher;
        _logger = logger;
    }

    public async Task Handle(ExecuteBulkConfigurationCommand request, CancellationToken cancellationToken)
    {
        // For simplicity, we process them sequentially. For higher throughput,
        // this could be parallelized with Task.WhenAll and proper error aggregation.
        foreach (var clientId in request.ClientIds)
        {
            try
            {
                var client = await _clientRepository.GetByIdAsync(clientId);
                if (client != null)
                {
                    client.UpdateConfiguration(request.Configuration);
                    await _clientRepository.UpdateAsync(client);
                    await _configPublisher.PublishUpdateAsync(client.Id, client.Configuration);
                    _logger.LogInformation("Successfully updated and published configuration for client {ClientId}", clientId);
                }
                else
                {
                    _logger.LogWarning("Could not apply bulk configuration. Client with ID {ClientId} not found", clientId);
                }
            }
            catch (Exception ex)
            {
                // Handle partial failures gracefully. Log the error and continue with the next client.
                _logger.LogError(ex, "Failed to update configuration for client {ClientId} during bulk operation", clientId);
            }
        }
    }
}