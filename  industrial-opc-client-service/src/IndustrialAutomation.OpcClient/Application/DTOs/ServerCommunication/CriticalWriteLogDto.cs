using System;

namespace IndustrialAutomation.OpcClient.Application.DTOs.ServerCommunication
{
    public record CriticalWriteLogDto(
        string ClientId,
        DateTime TimestampUtc,
        string TagId,
        object? OldValue,
        object NewValue,
        string? InitiatingUser,
        string? Context,
        bool Success,
        string? StatusCode, // OPC status code or internal status from write operation
        string? ErrorMessage
    );
}