using MediatR;
using System;

namespace ManagementService.Domain.Events;

/// <summary>
/// Notifies interested parties that a configuration migration job (identified by JobId) has started, including details like the original file name.
/// </summary>
public record ConfigurationMigrationInitiatedEvent(
    Guid JobId,
    string FileName
) : INotification;