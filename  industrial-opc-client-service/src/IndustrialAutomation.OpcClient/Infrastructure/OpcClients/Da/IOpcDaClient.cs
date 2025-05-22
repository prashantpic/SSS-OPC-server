using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Da
{
    public interface IOpcDaClient : IOpcConnection
    {
        Task<List<string>> BrowseAsync(string? parentItemId = null); // Returns list of Item IDs
        Task<List<OpcPointDto>> ReadAsync(List<string> itemIds);
        Task<List<WriteResponseDto>> WriteAsync(Dictionary<string, object> values); // Key: ItemId
    }
}