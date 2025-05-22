using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Hda;
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Hda
{
    public interface IOpcHdaClient : IOpcConnection
    {
        // HDA often has methods to get server capabilities, supported aggregates etc.
        // Task<List<string>> GetSupportedAggregatesAsync();

        Task<HdaReadResponseDto> ReadRawAsync(List<string> itemIds, DateTime startTime, DateTime endTime, bool includeBounds);
        Task<HdaReadResponseDto> ReadProcessedAsync(List<string> itemIds, DateTime startTime, DateTime endTime, string aggregationType, double resampleIntervalMs);
        // Potentially other read types like ReadAtTime, ReadModified, etc.
    }
}