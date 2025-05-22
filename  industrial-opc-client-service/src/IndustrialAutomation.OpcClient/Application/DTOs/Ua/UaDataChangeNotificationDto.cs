using IndustrialAutomation.OpcClient.Application.DTOs.Common;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaDataChangeNotificationDto
    {
        public string SubscriptionId { get; init; } = string.Empty; // Client-defined subscription ID
        public string ClientTagId { get; init; } = string.Empty; // Client's internal TagId for the monitored item
        public string NodeId { get; init; } = string.Empty; // OPC UA NodeId of the item that changed
        public OpcPointDto DataPoint { get; init; } = new OpcPointDto();
    }
}