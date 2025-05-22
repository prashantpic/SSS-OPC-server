using IndustrialAutomation.OpcClient.Application.DTOs.Configuration;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.ServerConnectivity.Grpc
{
    public interface IServerAppGrpcClient
    {
        // From opc_client_management.proto
        Task<ClientConfigurationDto?> GetConfigurationAsync(string clientId);
        Task<bool> SendClientStatusAsync(ClientHealthStatusDto status);
        Task<ModelUpdateRequestDto?> GetModelAsync(string modelName, string modelVersion); // Example for model download

        // From data_ingestion.proto (if gRPC is used for some data types)
        Task<bool> SendRealtimeDataAsync(RealtimeDataBatchDto batch); // Example
        Task<bool> SendHistoricalDataAsync(HistoricalDataBatchDto batch); // Example
        Task<bool> SendAlarmEventDataAsync(AlarmEventBatchDto batch); // Example
        Task<bool> SendSubscriptionStatusAsync(SubscriptionStatusDto status); // Example
        Task<bool> SendCriticalWriteLogAsync(CriticalWriteLogDto logEntry); // Example
        Task<bool> SendAuditEventAsync(AuditEventDto auditEvent); // Example
        Task<bool> SendEdgeAiOutputAsync(EdgeModelOutputDto output); // Example
    }
}