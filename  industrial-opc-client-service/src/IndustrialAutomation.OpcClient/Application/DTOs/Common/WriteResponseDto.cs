namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record WriteResponseDto
    {
        public string TagId { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = string.Empty;
        public bool Success { get; init; }
        public string StatusCode { get; init; } = string.Empty; // OPC Status Code (e.g., Opc.Ua.StatusCodes)
        public string? ErrorMessage { get; init; }
    }
}