using System;

namespace ManagementService.Api.V1.DTOs;

/// <summary>
/// Represents the request payload for initiating a configuration migration, including file content (e.g., as base64 string) and source format (CSV/XML).
/// </summary>
public record ConfigurationMigrationRequestDto(
    string FileContentBase64,
    string FileName,
    string SourceFormat
);