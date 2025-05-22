using IndustrialAutomation.OpcClient.Application.DTOs.Common;
using IndustrialAutomation.OpcClient.Application.DTOs.Hda; // For HdaReadResponseDto
using IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.Infrastructure.OpcClients.Hda
{
    public interface IOpcHdaClient : IOpcConnection
    {
        // itemIds are OPC HDA ItemIDs (string)
        Task<HdaReadResponseDto> ReadRawAsync(List<string> itemIds, DateTime startTime, DateTime endTime, bool includeBounds);
        Task<HdaReadResponseDto> ReadProcessedAsync(List<string> itemIds, DateTime startTime, DateTime endTime, string aggregationType, double resampleIntervalMs);
        // Potentially other HDA methods like ReadAtTime, ReadModified, etc.
        // Task<HdaReadResponseDto> ReadAtTimeAsync(List<string> itemIds, List<DateTime> timestamps);
        // Task<HdaReadResponseDto> ReadModifiedAsync(List<string> itemIds, DateTime startTime, DateTime endTime, uint maxValues);
        // Task<HdaReadResponseDto> ReadAttributeAsync(string itemId, DateTime startTime, DateTime endTime, List<uint> attributeIds);
    }
}