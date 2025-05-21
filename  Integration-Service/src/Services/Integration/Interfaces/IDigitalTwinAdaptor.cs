using IntegrationService.Adapters.DigitalTwin.Models; // Assuming DTOs will be in this namespace
using IntegrationService.Adapters.IoT.Models; // For IoTDataMessage and IoTCommand
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    public interface IDigitalTwinAdaptor
    {
        string PlatformType { get; } 

        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task SyncDataAsync(DigitalTwinUpdateRequest request, CancellationToken cancellationToken = default);
        Task<DigitalTwinModelInfo?> GetDigitalTwinModelInfoAsync(string twinId, CancellationToken cancellationToken = default);
        Task SendTelemetryAsync(string twinId, IoTDataMessage telemetry, CancellationToken cancellationToken = default);
        Task<IoTCommandResponse?> InvokeTwinCommandAsync(string twinId, IoTCommand command, CancellationToken cancellationToken = default); // Assuming IoTCommandResponse defined in IIoTPlatformAdaptor's scope or shared
        bool IsConnected { get; }
    }
}