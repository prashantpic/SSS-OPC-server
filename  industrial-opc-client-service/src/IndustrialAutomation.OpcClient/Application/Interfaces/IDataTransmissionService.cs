using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Interfaces;

/// <summary>
/// Defines the contract for forwarding various types of data 
/// from the OPC client to the central server application.
/// </summary>
public interface IDataTransmissionService
{
    Task SendRealtimeDataBatchAsync(RealtimeDataBatchDto batch);

    Task SendHistoricalDataBatchAsync(HistoricalDataBatchDto batch);

    Task SendAlarmEventBatchAsync(AlarmEventBatchDto batch);

    Task SendClientHealthStatusAsync(ClientHealthStatusDto status);

    Task SendSubscriptionStatusAsync(SubscriptionStatusDto status);

    Task SendCriticalWriteLogAsync(CriticalWriteLogDto log);

    Task SendAuditEventAsync(AuditEventDto auditEvent);

    Task SendEdgeAiOutputAsync(EdgeModelOutputDto output);
}