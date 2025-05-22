using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record ReadRequestDto(
        string ServerId,
        List<string> TagIds // These are internal TagIds, to be mapped to OpcAddress by the service
    );
}