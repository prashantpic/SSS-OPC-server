namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaBrowseNodeDto(
        string NodeId,
        string DisplayName,
        string NodeClass, // e.g., "Object", "Variable", "Method", "View", "ObjectType", "VariableType", "ReferenceType", "DataType"
        bool HasChildren // Or IsLeaf as per SDS, HasChildren might be more direct from some SDKs
    );
}