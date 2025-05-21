using System;
using System.Collections.Generic;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents the status (e.g., Pending, InProgress, Completed, Failed), outcome, and details of a configuration migration job for API communication.
/// </summary>
public record ConfigurationMigrationJobDto(
    Guid JobId,
    string FileName,
    string SourceFormat,
    string Status,
    string? Details, // e.g., success message or error summary
    List<string> ValidationMessages,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt
);