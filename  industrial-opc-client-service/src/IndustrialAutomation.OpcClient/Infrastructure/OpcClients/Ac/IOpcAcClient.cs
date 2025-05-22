using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ac
{
    public interface IOpcAcClient : IOpcConnection
    {
        // Event to notify subscribers (e.g., OpcCommunicationService) of new alarms/events
        event EventHandler<AcAlarmEventDto> AlarmEventReceived;

        // Method to start subscribing to events from the A&C server.
        // Specific subscription parameters (filters, areas) might be passed here or configured internally.
        Task<bool> SubscribeToEventsAsync(); // Or pass filter criteria

        // Method to acknowledge an event.
        // eventId typically corresponds to the AcAlarmEventDto.EventId or a server-specific cookie.
        Task<bool> AcknowledgeEventAsync(string eventId, string user, string comment);

        // Potentially methods for other A&C operations like:
        // - Enable/Disable conditions
        // - GetConditionState
        // - Refresh (for current active alarms)
    }
}