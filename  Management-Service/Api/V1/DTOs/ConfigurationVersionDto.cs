using System;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents a single version of a client's configuration (e.g., VersionNumber, Content, CreatedAt, IsActive) for API communication.
/// </summary>
public record ConfigurationVersionDto(
    Guid Id,
    int VersionNumber,
    string Content,
    DateTimeOffset CreatedAt,
    bool IsActive // Indicates if this is the active version for its parent configuration
);