using IndustrialAutomation.OpcClient.Application.DTOs.Ac;
using System;
using System.Collections.Generic;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record AlarmEventBatchDto
    {
        public string ClientId { get; set; } = string.Empty; // Set by DataTransmissionService
        public DateTime BatchTimestampUtc { get; init; } = DateTime.UtcNow;
        public List<AcAlarmEventDto> AlarmEvents { get; init; } = new List<AcAlarmEventDto>();
    }
}