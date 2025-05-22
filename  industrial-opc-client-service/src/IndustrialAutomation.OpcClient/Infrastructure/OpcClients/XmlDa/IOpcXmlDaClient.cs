using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.XmlDa
{
    public interface IOpcXmlDaClient : IOpcConnection
    {
        // XML-DA browse is usually less common or done via GetStatus for server info
        // Task<List<string>> BrowseAsync(string? parentItemId = null); 
        Task<List<OpcPointDto>> ReadAsync(List<string> itemIds);
        Task<List<WriteResponseDto>> WriteAsync(Dictionary<string, object> values); // Key: ItemId
    }
}