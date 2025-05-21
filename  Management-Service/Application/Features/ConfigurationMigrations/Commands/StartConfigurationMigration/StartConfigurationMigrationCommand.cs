using MediatR;
using System;

namespace ManagementService.Application.Features.ConfigurationMigrations.Commands.StartConfigurationMigration;

// MediatR command representing a request to start a configuration migration process from a file.
public record StartConfigurationMigrationCommand(
    byte[] FileContent,
    string FileName,
    string SourceFormat
) : IRequest<Guid>; // Returns the ID of the created ConfigurationMigrationJob