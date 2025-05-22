using System;
using System.Collections.Generic;
using IndustrialAutomation.OpcClient.Application.DTOs.Ac;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record AlarmEventBatchDto(
        string ClientId,
        DateTime BatchTimestampUtc,
        List<AcAlarmEventDto> AlarmEvents
    );
}