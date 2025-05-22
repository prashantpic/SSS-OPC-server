using IndustrialAutomation.OpcClient.Application.DTOs.Common;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaDataChangeNotificationDto(
        string SubscriptionId,
        string ItemId, // Client-defined ItemId from UaMonitoredItemDto
        OpcPointDto DataPoint
    );
}