using IndustrialAutomation.OpcClient.Application.DTOs.Common; // For ServerConnectionConfigDto
using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication; // For ClientHealthStatusDto parts
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common
{
    public interface IOpcConnection : IDisposable
    {
        string ServerId { get; }
        Task<bool> ConnectAsync(ServerConnectionConfigDto config);
        Task DisconnectAsync();
        OpcConnectionStatus GetStatus(); // More detailed status
    }

    public record OpcConnectionStatus
    {
        public bool IsConnected { get; init; }
        public string StatusMessage { get; init; } = "Unknown";
        public DateTime LastStatusChangeUtc { get; init; } = DateTime.UtcNow;
        public string? LastError { get; init; }
    }
}