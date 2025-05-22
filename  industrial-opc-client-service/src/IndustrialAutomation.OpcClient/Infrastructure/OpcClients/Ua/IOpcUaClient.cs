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
        // nodeIds are OPC UA NodeId strings (e.g., "ns=2;s=MyVariable")
        Task<List<OpcPointDto>> ReadAsync(List<string> nodeIds);
        // Dictionary key is NodeId string, value is the object to write
        Task<List<WriteResponseDto>> WriteAsync(Dictionary<string, object> nodeValues);
        // parentNodeId can be null to browse root objects.
        Task<List<UaBrowseNodeDto>> BrowseAsync(string? parentNodeId = null);

        Task<Opc.Ua.Client.Subscription> CreateUaSubscriptionAsync(UaSubscriptionConfigDto config, Action<Opc.Ua.Client.Subscription, Opc.Ua.NotificationMessage> notificationHandler);
        Task<bool> AddMonitoredItemsAsync(Opc.Ua.Client.Subscription subscription, List<UaMonitoredItemDto> items, Action<Opc.Ua.Client.MonitoredItem, Opc.Ua.MonitoredItemNotificationEventArgs> itemNotificationHandler);
        Task<bool> RemoveMonitoredItemsAsync(Opc.Ua.Client.Subscription subscription, List<Opc.Ua.Client.MonitoredItem> items);
        Task<bool> DeleteSubscriptionAsync(Opc.Ua.Client.Subscription subscription);

        Opc.Ua.Client.Session? GetSession(); // To allow UaSubscriptionManager to interact with the session directly for subscriptions
    }
}