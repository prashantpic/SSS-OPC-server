using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ac
{
    public interface IOpcAcClient : IOpcConnection
    {
        event Action<AcAlarmEventDto> AlarmEventReceived;

        Task SubscribeToEventsAsync(); // Or provide configuration for filtering events
        Task<bool> AcknowledgeEventAsync(string eventId, string user, string comment);
        // Potentially methods for other A&C features like Enable/Disable conditions, Refresh, etc.
    }
}