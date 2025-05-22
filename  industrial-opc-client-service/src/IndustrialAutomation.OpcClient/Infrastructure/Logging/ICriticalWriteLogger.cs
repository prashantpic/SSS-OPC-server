using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;

namespace IndustrialAutomation.OpcClient.Infrastructure.Logging
{
    public interface ICriticalWriteLogger
    {
        void LogCriticalWrite(CriticalWriteLogDto logEntry);
    }
}