using System;
using System.Collections.Generic;
using IndustrialAutomation.OpcClient.Application.DTOs.Common;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record RealtimeDataBatchDto(
        string ClientId,
        DateTime BatchTimestampUtc,
        List<OpcPointDto> DataPoints
    );
}