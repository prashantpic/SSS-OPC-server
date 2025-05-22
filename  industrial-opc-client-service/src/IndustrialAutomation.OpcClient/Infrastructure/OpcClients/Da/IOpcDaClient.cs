using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Da
{
    public interface IOpcDaClient : IOpcConnection
    {
        // itemIds are OPC DA ItemIDs (string)
        Task<List<OpcPointDto>> ReadAsync(List<string> itemIds);
        // Dictionary key is ItemID, value is the object to write
        Task<List<WriteResponseDto>> WriteAsync(Dictionary<string, object> values);
        // parentItemId can be null or empty to browse root. Returns list of ItemIDs or browse elements.
        Task<List<string>> BrowseAsync(string? parentItemId = null); // Simplified to return item names/IDs
        // Could return a more structured DTO like UaBrowseNodeDto if DA server supports rich browsing
    }
}