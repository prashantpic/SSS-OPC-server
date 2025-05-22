using IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication;
using IndustrialAutomation.OpcClient.Application.DTOs.EdgeAi; // For EdgeModelOutputDto
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for forwarding various types of data 
    /// from the OPC client to the central server application.
    /// </summary>
    public interface IDataTransmissionService
    {
        /// <summary>
        /// Sends a batch of real-time data to the server.
        /// </summary>
        /// <param name="batch">The real-time data batch.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendRealtimeDataBatchAsync(RealtimeDataBatchDto batch);

        /// <summary>
        /// Sends a batch of historical data to the server.
        /// </summary>
        /// <param name="batch">The historical data batch.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendHistoricalDataBatchAsync(HistoricalDataBatchDto batch);

        /// <summary>
        /// Sends a batch of alarm and event data to the server.
        /// </summary>
        /// <param name="batch">The alarm and event data batch.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAlarmEventBatchAsync(AlarmEventBatchDto batch);

        /// <summary>
        /// Sends the client's health status to the server.
        /// </summary>
        /// <param name="status">The client health status.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendClientHealthStatusAsync(ClientHealthStatusDto status);

        /// <summary>
        /// Sends the status of an OPC UA subscription to the server.
        /// </summary>
        /// <param name="status">The subscription status.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendSubscriptionStatusAsync(SubscriptionStatusDto status);

        /// <summary>
        /// Sends a critical write operation log to the server.
        /// </summary>
        /// <param name="log">The critical write log entry.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendCriticalWriteLogAsync(CriticalWriteLogDto log);

        /// <summary>
        /// Sends a generic audit event to the server.
        /// </summary>
        /// <param name="auditEvent">The audit event.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendAuditEventAsync(AuditEventDto auditEvent);

        /// <summary>
        /// Sends the output of an edge AI model execution to the server.
        /// </summary>
        /// <param name="output">The edge AI model output.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEdgeAiOutputAsync(EdgeModelOutputDto output);
    }
}