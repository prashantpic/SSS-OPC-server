using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi; // For EdgeModelOutputDto
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Messaging
{
    public interface IServerAppMessageProducer : IDisposable
    {
        Task PublishRealtimeDataAsync(RealtimeDataBatchDto batch);
        Task PublishHistoricalDataAsync(HistoricalDataBatchDto batch);
        Task PublishAlarmEventAsync(AlarmEventBatchDto batch);
        Task PublishCriticalWriteLogAsync(CriticalWriteLogDto log);
        Task PublishAuditEventAsync(AuditEventDto auditEvent);
        Task PublishSubscriptionStatusAsync(SubscriptionStatusDto status);
        Task PublishEdgeAiOutputAsync(EdgeModelOutputDto output);

        Task<bool> IsConnectedAsync(); // To check broker connectivity
    }
}