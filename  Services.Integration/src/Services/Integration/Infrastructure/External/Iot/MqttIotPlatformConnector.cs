using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using Opc.System.Services.Integration.Application.Contracts.External;
using Opc.System.Services.Integration.Domain.Aggregates;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Opc.System.Services.Integration.Application.Contracts.Persistence; // Assuming this contract exists

namespace Opc.System.Services.Integration.Infrastructure.External.Iot;

/// <summary>
/// Implementation of IIotPlatformConnector using the MQTT protocol with the MQTTnet library.
/// </summary>
public class MqttIotPlatformConnector : IIotPlatformConnector
{
    private readonly IIntegrationConnectionRepository _connectionRepository; // Assume repo exists
    private readonly ILogger<MqttIotPlatformConnector> _logger;
    private readonly MqttFactory _mqttFactory;

    public MqttIotPlatformConnector(IIntegrationConnectionRepository connectionRepository, ILogger<MqttIotPlatformConnector> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
        _mqttFactory = new MqttFactory();
    }

    /// <inheritdoc />
    public async Task<bool> SendDataAsync(Guid connectionId, string payload)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null || !connection.IsEnabled)
        {
            _logger.LogWarning("MQTT connection {ConnectionId} not found or is disabled. Cannot send data.", connectionId);
            return false;
        }

        using var mqttClient = _mqttFactory.CreateMqttClient();
        try
        {
            var options = BuildMqttClientOptions(connection);
            var connectResult = await mqttClient.ConnectAsync(options, CancellationToken.None);

            if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
            {
                _logger.LogError("Failed to connect to MQTT broker for connection {ConnectionId}. Result: {ResultCode}", connectionId, connectResult.ResultCode);
                return false;
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{GetClientId(connection)}/messages/events/") // Example topic structure
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            var publishResult = await mqttClient.PublishAsync(message, CancellationToken.None);

            await mqttClient.DisconnectAsync();

            if (publishResult.IsSuccess)
            {
                _logger.LogInformation("Successfully published message to MQTT broker for connection {ConnectionId}.", connectionId);
                return true;
            }

            _logger.LogWarning("Failed to publish message for connection {ConnectionId}. Reason: {Reason}", connectionId, publishResult.ReasonString);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while sending data via MQTT for connection {ConnectionId}.", connectionId);
            return false;
        }
    }

    /// <inheritdoc />
    public Task StartReceivingAsync(Guid connectionId, Func<string, Task> onMessageReceived)
    {
        _logger.LogWarning("StartReceivingAsync is not fully implemented for MqttIotPlatformConnector. It requires a long-running managed client implementation.");
        // A full implementation would create a ManagedMqttClient, configure it to stay connected,
        // subscribe to a command topic, and handle the onMessageReceived callback in its
        // ApplicationMessageReceivedAsync handler. This would typically be started in a BackgroundService.
        return Task.CompletedTask;
    }

    private MqttClientOptions BuildMqttClientOptions(IntegrationConnection connection)
    {
        var config = JsonSerializer.Deserialize<MqttSecurityConfiguration>(connection.SecurityConfiguration.RootElement.GetRawText());
        if (config == null)
            throw new InvalidOperationException($"Invalid security configuration for connection {connection.Id}");

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(connection.Endpoint)
            .WithClientId(config.ClientId);

        if (!string.IsNullOrEmpty(config.Username))
        {
            optionsBuilder.WithCredentials(config.Username, config.Password);
        }

        if (config.UseTls)
        {
            optionsBuilder.WithTls();
        }

        return optionsBuilder.Build();
    }
    
    private string GetClientId(IntegrationConnection connection)
    {
        var config = JsonSerializer.Deserialize<MqttSecurityConfiguration>(connection.SecurityConfiguration.RootElement.GetRawText());
        return config?.ClientId ?? $"client_{connection.Id}";
    }
    
    // Helper class for deserializing security config
    private class MqttSecurityConfiguration
    {
        public string ClientId { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool UseTls { get; set; }
    }
}