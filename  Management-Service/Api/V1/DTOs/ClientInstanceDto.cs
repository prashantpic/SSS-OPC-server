using System;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents client instance data (e.g., Id, Name, LastSeen, Status, CurrentConfigurationVersion) for API communication.
/// </summary>
public record ClientInstanceDto(
    Guid Id,
    string Name,
    string Status, // String representation of ClientStatusValueObject
    DateTimeOffset? LastSeen,
    Guid? CurrentConfigurationVersionId
);