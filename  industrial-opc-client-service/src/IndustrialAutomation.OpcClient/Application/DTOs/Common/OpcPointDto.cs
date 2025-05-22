using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record OpcPointDto(
        string TagId,
        object Value,
        DateTime Timestamp,
        string QualityStatus);
}