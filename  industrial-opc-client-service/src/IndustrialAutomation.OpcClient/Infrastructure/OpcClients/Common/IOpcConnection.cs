using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using System;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common
{
    public interface IOpcConnection : IDisposable
    {
        string ServerId { get; }
        bool IsConnected { get; }
        Task ConnectAsync(ServerConnectionConfigDto config);
        Task DisconnectAsync();
        // GetStatus() might be too generic. IsConnected and specific exceptions are often better.
        // Task<string> GetStatusAsync(); 
    }
}