namespace IndustrialAutomation.OpcClient.Application.DTOs.Ua
{
    public record UaBrowseNodeDto
    {
        public string NodeId { get; init; } = string.Empty; // e.g., "ns=2;s=MyVariable"
        public string DisplayName { get; init; } = string.Empty;
        public string NodeClass { get; init; } = string.Empty; // e.g., "Variable", "Object", "Method"
        public bool HasChildren { get; init; } // Indicates if this node can be further browsed
        public string? Description { get; init; }
    }
}