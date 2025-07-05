using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Opc.System.Services.Integration.Application.Contracts.External;
using Opc.System.Services.Integration.Application.Contracts.Persistence;
using Opc.System.Services.Integration.Domain.Aggregates;
using Polly;
using Polly.Retry;
using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Opc.System.Services.Integration.Infrastructure.External.DigitalTwin;

/// <summary>
/// Implementation of the IDigitalTwinAdapter for Azure Digital Twins.
/// </summary>
public class AzureDigitalTwinAdapter : IDigitalTwinAdapter
{
    private readonly IIntegrationConnectionRepository _connectionRepository;
    private readonly ILogger<AzureDigitalTwinAdapter> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AzureDigitalTwinAdapter(IIntegrationConnectionRepository connectionRepository, ILogger<AzureDigitalTwinAdapter> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<RequestFailedException>(ex => ex.Status == 429 || ex.Status >= 500) // Throttling or server error
            .Or<SocketException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Retry {RetryCount} for Azure Digital Twin operation due to {ExceptionType}. Waiting {TimeSpan} before next try.", retryCount, exception.GetType().Name, timeSpan);
                });
    }

    /// <inheritdoc />
    public async Task SendDataAsync(Guid connectionId, string twinId, string payload)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null || !connection.IsEnabled)
        {
            _logger.LogWarning("Digital Twin connection {ConnectionId} not found or is disabled.", connectionId);
            return;
        }

        try
        {
            var config = GetSecurityConfig(connection);
            var credential = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret);
            var client = new DigitalTwinsClient(new Uri(config.AdtInstanceUrl), credential);
            
            var patchDocument = JsonSerializer.Deserialize<JsonPatchDocument>(payload);

            _logger.LogInformation("Updating digital twin {TwinId} on instance {InstanceUrl}", twinId, config.AdtInstanceUrl);
            
            await _retryPolicy.ExecuteAsync(async () => 
            {
                await client.UpdateDigitalTwinAsync(twinId, patchDocument);
            });

            _logger.LogInformation("Successfully updated digital twin {TwinId}", twinId);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Failed to deserialize payload into a JsonPatchDocument for twin {TwinId}.", twinId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending data to Azure Digital Twin {TwinId} for connection {ConnectionId}", twinId, connectionId);
            throw;
        }
    }

    /// <inheritdoc />
    public Task StartReceivingCommandsAsync(Guid connectionId, Func<string, Task> onCommandReceived)
    {
        _logger.LogWarning("StartReceivingCommandsAsync is not implemented. A full implementation requires setting up an Event Grid/Hub subscription and a listener to process incoming events from the twin graph.");
        return Task.CompletedTask;
    }

    private DigitalTwinSecurityConfiguration GetSecurityConfig(IntegrationConnection connection)
    {
        var config = JsonSerializer.Deserialize<DigitalTwinSecurityConfiguration>(connection.SecurityConfiguration.RootElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (config == null || string.IsNullOrWhiteSpace(config.AdtInstanceUrl) || string.IsNullOrWhiteSpace(config.TenantId) || string.IsNullOrWhiteSpace(config.ClientId) || string.IsNullOrWhiteSpace(config.ClientSecret))
        {
            throw new InvalidOperationException($"Invalid or incomplete security configuration for Digital Twin connection {connection.Id}");
        }
        return config;
    }

    private class DigitalTwinSecurityConfiguration
    {
        public string AdtInstanceUrl { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}