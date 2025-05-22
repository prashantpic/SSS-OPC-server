using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record ReadResponseDto(
        string ServerId,
        List<OpcPointDto> Values,
        bool Success,
        string? StatusCode, // OPC status code or internal status
        string? ErrorMessage
    );
}