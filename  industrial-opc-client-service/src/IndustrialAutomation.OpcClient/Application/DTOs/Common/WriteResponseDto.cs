namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record WriteResponseDto(
        string TagId,
        bool Success,
        string? StatusCode, // OPC status code or internal status
        string? ErrorMessage
    );
}