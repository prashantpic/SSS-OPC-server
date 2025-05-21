using System;
using System.Collections.Generic;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents the request payload for bulk configuration operations, specifying target client IDs and the configuration version ID to apply.
/// </summary>
public record BulkConfigurationRequestDto(
    List<Guid> ClientInstanceIds,
    Guid ConfigurationVersionId
);