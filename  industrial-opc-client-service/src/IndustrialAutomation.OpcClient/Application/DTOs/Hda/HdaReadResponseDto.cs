using System.Collections.Generic;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Hda
{
    public record HdaReadResponseDto(
        string ServerId,
        List<OpcPointDto> HistoricalData, // Flattened list; can be grouped by TagId by consumer if needed
        bool Success,
        string? StatusCode, // OPC HDA status code or internal status
        string? ErrorMessage
    );
}