using MQTTnet;
using MQTTnet.Client;
using Services.Integration.Application.Interfaces;
using Services.Integration.Domain.Aggregates;
using Services.Integration.Domain.ValueObjects;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography;
using System.Text;

namespace Services.Integration.Infrastructure.Connectors.Iot;

/// <summary>
/// Implements the IIotPlatformConnector interface for Azure IoT Hub using the MQTTnet library.
/// </summary>
public class AzureIotConnector : IIotPlatformConnector
{
    private readonly ILogger<AzureIotConnector> _logger;
    private readonly IMqttFactory _mqttFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureIotConnector"/> class.
    /// </summary>
    /// <param name="logger">The logger for structured logging.</param>
    /// <param name="mqttFactory">The factory to create MQTT clients.</param>
    public AzureIotConnector(ILogger<AzureIotConnector> logger, IMqttFactory mqttFactory)
    {
        _logger = logger;
        _mqttFactory = mqttFactory;
    }

    /// <inheritdoc />
    public bool Supports(EndpointType type) => type == EndpointType.AzureIot;

    /// <inheritdoc />
    public async Task<bool> SendDataAsync(IntegrationEndpoint endpoint, string payload, CancellationToken cancellationToken)
    {
        // Azure IoT Hub connection details are expected in the endpoint's Address properties
        if (!endpoint.Address.Properties.TryGetValue("HostName", out var hostName) ||
            !endpoint.Address.Properties.TryGetValue("DeviceId", out var deviceId) ||
            !endpoint.Address.Properties.TryGetValue("SharedAccessKey", out var sharedAccessKey))
        {
            _logger.LogError("Azure IoT Hub configuration is missing required properties (HostName, DeviceId, SharedAccessKey) for EndpointId: {EndpointId}", endpoint.Id);
            return false;
        }

        var mqttClient = _mqttFactory.CreateMqttClient();
        
        try
        {
            var sasToken = BuildAzureSasToken(hostName, deviceId, sharedAccessKey);
            var topic = $"devices/{deviceId}/messages/events/";

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(hostName, 8883)
                .WithClientId(deviceId)
                .WithCredentials($"{hostName}/{deviceId}/?api-version=2021-04-12", sasToken)
                .WithTlsOptions(o =>
                {
                    o.UseTls = true;
                    // Azure requires TLS 1.2
                    o.SslProtocol = System.Security.Authentication.SslProtocols.Tls12; 
                    // Optional: Server certificate validation can be customized here if needed
                    o.CertificateValidationHandler = _ => true;
                })
                .WithCleanSession()
                .Build();
            
            _logger.LogInformation("Connecting to Azure IoT Hub '{HostName}' with DeviceId '{DeviceId}'", hostName, deviceId);
            var connectResult = await mqttClient.ConnectAsync(options, cancellationToken);
            if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
            {
                _logger.LogError("Failed to connect to Azure IoT Hub: {ResultCode} - {ReasonString}", connectResult.ResultCode, connectResult.ReasonString);
                return false;
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            _logger.LogDebug("Publishing message to topic '{Topic}'", topic);
            var publishResult = await mqttClient.PublishAsync(message, cancellationToken);
            
            if (publishResult.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                 _logger.LogWarning("Failed to publish MQTT message to Azure IoT Hub. Reason: {ReasonCode}", publishResult.ReasonCode);
                 return false;
            }

            _logger.LogInformation("Successfully published message for DeviceId '{DeviceId}'", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending data to Azure IoT Hub for EndpointId: {EndpointId}", endpoint.Id);
            return false;
        }
        finally
        {
            if (mqttClient.IsConnected)
            {
                await mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build(), CancellationToken.None);
                _logger.LogDebug("MQTT client disconnected from Azure IoT Hub.");
            }
            mqttClient.Dispose();
        }
    }
    
    /// <summary>
    /// Builds a Shared Access Signature (SAS) token for Azure IoT Hub authentication.
    /// </summary>
    private static string BuildAzureSasToken(string hostName, string deviceId, string sharedAccessKey, int TtlMinutes = 60)
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(TtlMinutes).ToUnixTimeSeconds();
        var resourceUri = $"{hostName}/devices/{deviceId}";
        var stringToSign = $"{Uri.EscapeDataString(resourceUri)}\n{expiry}";
        
        using var hmac = new HMACSHA256(Convert.FromBase64String(sharedAccessKey));
        var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

        return $"SharedAccessSignature sr={Uri.EscapeDataString(resourceUri)}&sig={Uri.EscapeDataString(signature)}&se={expiry}";
    }
}