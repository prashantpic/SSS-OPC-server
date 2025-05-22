using IndustrialAutomation.OpcClient.Domain.Enums;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record TagDefinitionDto(
        string TagId,         // Unique identifier for the tag within this client application
        string OpcAddress,    // NodeId for UA, ItemId for DA/XML-DA
        string ServerId,      // Identifies which OpcServerConfig this tag belongs to
        OpcStandard OpcStandard, // The OPC standard of the server this tag belongs to
        string DataType,      // Expected .NET data type as string (e.g., "System.Int32", "System.Double")
        double ScalingFactor = 1.0,
        double Offset = 0.0,
        bool IsActive = true,
        bool IsWritable = false, // Indicates if the tag supports write operations
        Dictionary<string, string>? Properties = null // For protocol-specific or custom properties
    );
}