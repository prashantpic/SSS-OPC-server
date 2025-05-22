using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Ua;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Ua
{
    public interface IOpcUaClient : IOpcConnection
    {
        // Event for data change notifications
        event Action<UaDataChangeNotificationDto> DataChangeReceived;
        // Event for subscription status changes
        event Action<SubscriptionStatusDto> SubscriptionStatusChanged;


        Task<UaBrowseNodeDto[]> BrowseAsync(string? nodeId = null);
        Task<List<OpcPointDto>> ReadAsync(List<string> nodeIds);
        Task<List<WriteResponseDto>> WriteAsync(Dictionary<string, object> nodeValues); // Key: NodeId

        Task<string> CreateSubscriptionAsync(UaSubscriptionConfigDto config);
        Task<bool> ModifySubscriptionAsync(UaSubscriptionConfigDto config); // For changing publishing interval etc.
        Task<bool> RemoveSubscriptionAsync(string subscriptionId); // Client-generated subscriptionId from config

        Task<bool> AddMonitoredItemsAsync(string subscriptionId, List<UaMonitoredItemDto> items);
        Task<bool> RemoveMonitoredItemsAsync(string subscriptionId, List<string> itemClientHandles); // itemClientHandles are UaMonitoredItemDto.ItemId
    }
}