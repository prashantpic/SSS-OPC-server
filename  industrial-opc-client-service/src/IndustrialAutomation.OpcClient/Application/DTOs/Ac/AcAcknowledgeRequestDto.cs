namespace IndustrialAutomation.OpcClient.Application.DTOs.Ac
{
    public record AcAcknowledgeRequestDto
    {
        public string ServerId { get; init; } = string.Empty;
        public string EventId { get; init; } = string.Empty; // EventId from AcAlarmEventDto (or internal representation)
        public string? ConditionName { get; init; } // Often needed for OPC UA A&C acknowledgement
        public string User { get; init; } = "System"; // User performing the acknowledgement
        public string Comment { get; init; } = string.Empty; // Comment for the acknowledgement
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    }
}