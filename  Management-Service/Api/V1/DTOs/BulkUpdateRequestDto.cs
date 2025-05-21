using System;
using System.Collections.Generic;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents the request payload for bulk software update operations, specifying target client IDs and update package information (e.g., version, URL).
/// </summary>
public record BulkUpdateRequestDto(
    List<Guid> ClientInstanceIds,
    string UpdatePackageUrl,
    string TargetVersion
);