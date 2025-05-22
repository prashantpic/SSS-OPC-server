using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication; // For CriticalWriteLogDto
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.Logging
{
    public interface ICriticalWriteLogger
    {
        Task LogCriticalWriteAsync(CriticalWriteLogDto logEntry);
    }
}