using IndustrialAutomation.OpcClient.Domain.Enums;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record TagDefinitionDto
    {
        public string TagId { get; init; } = string.Empty; // Unique identifier within this client
        public string OpcAddress { get; init; } = string.Empty; // NodeId for UA, ItemId for DA/XML-DA
        public string ServerId { get; init; } = string.Empty; // Refers to ServerConnectionConfigDto.ServerId
        public OpcStandard OpcStandard { get; init; } = OpcStandard.Ua; // To know which client to use
        public string DataType { get; init; } = "System.Object"; // Expected .NET data type after conversion
        public double ScalingFactor { get; init; } = 1.0;
        public double Offset { get; init; } = 0.0;
        public bool IsActive { get; init; } = true;
        public bool IsReadable { get; init; } = true;
        public bool IsWritable { get; init; } = false;
        public string? Description { get; init; }
        public Dictionary<string, string>? ExtendedProperties { get; init; } // For protocol-specific needs
    }
}