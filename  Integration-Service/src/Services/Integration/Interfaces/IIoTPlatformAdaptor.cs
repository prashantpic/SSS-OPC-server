using IntegrationService.Adapters.IoT.Models; // Assuming DTOs will be in this namespace
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    // Minimal placeholder for IoTCommandResponse until fully defined
    public record IoTCommandResponse(string CorrelationId, bool Success, string? Message, object? Payload);

    public interface IIoTPlatformAdaptor
    {
        string PlatformType { get; } // e.g., "MQTT", "AMQP", "HTTP"

        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task PublishTelemetryAsync(IoTDataMessage message, CancellationToken cancellationToken = default);
        Task SubscribeToCommandsAsync(Func<IoTCommand, Task> commandHandler, CancellationToken cancellationToken = default);
        Task SendIoTCommandResponseAsync(IoTCommandResponse response, CancellationToken cancellationToken = default);
        bool IsConnected { get; }
    }
}