namespace IndustrialAutomation.OpcClient.Application.DTOs.Ac
{
    public record AcAcknowledgeRequestDto(
        string ServerId,
        string EventId,
        string User,
        string Comment
    );
}