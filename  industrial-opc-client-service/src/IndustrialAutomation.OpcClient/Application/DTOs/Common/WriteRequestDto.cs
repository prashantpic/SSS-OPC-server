using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.Common
{
    public record WriteRequestDto(
        string TagId,
        object Value,
        DateTime? Timestamp, // Optional: Some OPC standards might allow writing timestamp
        string? InitiatingUser, // User context for auditing
        string? Context // Additional context for the write operation
    );
}