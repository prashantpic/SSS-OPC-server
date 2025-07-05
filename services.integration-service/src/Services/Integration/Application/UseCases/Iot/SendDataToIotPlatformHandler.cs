using Services.Integration.Application.Interfaces;
using Services.Integration.Domain.Aggregates;
using System.ComponentModel.DataAnnotations;

namespace Services.Integration.Application.UseCases.Iot;

// Using a record for immutable command properties
public record SendDataToIotPlatformCommand(Guid EndpointId, [Required] string Payload);

// Placeholder for repository interface
public interface IIntegrationEndpointRepository
{
    Task<IntegrationEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

// Placeholder for a custom exception
public class EndpointNotFoundException : Exception
{
    public EndpointNotFoundException(Guid endpointId) : base($"IntegrationEndpoint with ID '{endpointId}' was not found.") { }
}


/// <summary>
/// Handles the command to send a data payload to a configured IoT platform.
/// It orchestrates the process of retrieving endpoint configuration and using the correct connector.
/// </summary>
public class SendDataToIotPlatformHandler
{
    private readonly IEnumerable<IIotPlatformConnector> _iotConnectors;
    private readonly IIntegrationEndpointRepository _endpointRepository;
    private readonly ILogger<SendDataToIotPlatformHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendDataToIotPlatformHandler"/> class.
    /// </summary>
    /// <param name="iotConnectors">A collection of available IoT platform connectors (injected by DI).</param>
    /// <param name="endpointRepository">The repository for accessing integration endpoint configurations.</param>
    /// <param name="logger">The logger for structured logging.</param>
    public SendDataToIotPlatformHandler(
        IEnumerable<IIotPlatformConnector> iotConnectors,
        IIntegrationEndpointRepository endpointRepository,
        ILogger<SendDataToIotPlatformHandler> logger)
    {
        _iotConnectors = iotConnectors;
        _endpointRepository = endpointRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the send data command.
    /// </summary>
    /// <param name="command">The command containing the endpoint ID and data payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if successful.</returns>
    /// <exception cref="EndpointNotFoundException">Thrown if the specified endpoint ID does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a suitable connector for the endpoint type cannot be found.</exception>
    public async Task<bool> Handle(SendDataToIotPlatformCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SendDataToIotPlatformCommand for EndpointId: {EndpointId}", command.EndpointId);

        var endpoint = await _endpointRepository.GetByIdAsync(command.EndpointId, cancellationToken);
        if (endpoint is null)
        {
            throw new EndpointNotFoundException(command.EndpointId);
        }

        if (!endpoint.IsEnabled)
        {
            _logger.LogWarning("Attempted to send data to disabled EndpointId: {EndpointId}", command.EndpointId);
            return false;
        }

        var connector = _iotConnectors.SingleOrDefault(c => c.Supports(endpoint.EndpointType));
        if (connector is null)
        {
            _logger.LogError("No IIotPlatformConnector found for EndpointType: {EndpointType} (EndpointId: {EndpointId})", endpoint.EndpointType.Name, endpoint.Id);
            throw new InvalidOperationException($"A connector for endpoint type '{endpoint.EndpointType.Name}' is not registered.");
        }
        
        _logger.LogInformation("Using connector '{ConnectorType}' for EndpointId: {EndpointId}", connector.GetType().Name, command.EndpointId);

        try
        {
            var success = await connector.SendDataAsync(endpoint, command.Payload, cancellationToken);
            if (success)
            {
                _logger.LogInformation("Successfully sent data to EndpointId: {EndpointId}", command.EndpointId);
            }
            else
            {
                _logger.LogWarning("Connector failed to send data for EndpointId: {EndpointId}", command.EndpointId);
            }
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while sending data via connector for EndpointId: {EndpointId}", command.EndpointId);
            // Depending on requirements, you might re-throw or return false.
            // Re-throwing is often better to let higher-level handlers decide on the response.
            throw; 
        }
    }
}