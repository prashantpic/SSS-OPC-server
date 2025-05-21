using System;
using System.Collections.Generic;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents client configuration data (e.g., Id, ClientInstanceId, Name, List of Versions) for API communication.
/// </summary>
public record ClientConfigurationDto(
    Guid Id,
    Guid ClientInstanceId,
    string Name,
    List<ConfigurationVersionDto> Versions,
    Guid? ActiveVersionId
);