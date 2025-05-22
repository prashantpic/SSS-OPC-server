using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.XmlDa
{
    public interface IOpcXmlDaClient : IOpcConnection
    {
        // itemIds are OPC XML-DA ItemNames (string)
        Task<List<OpcPointDto>> ReadAsync(List<string> itemNames);
        // Dictionary key is ItemName, value is the object to write
        Task<List<WriteResponseDto>> WriteAsync(Dictionary<string, object> values);
        // XML-DA browse is typically part of Read/Subscription or a separate "GetProperties" call.
        // For simplicity, not including a dedicated browse here as it's less common than UA/DA.
    }
}