using MediatR;
using System;

namespace ManagementService.Domain.Events;

/// <summary>
/// Notifies other parts of the system or external listeners that a new bulk operation (identified by JobId) has commenced.
/// </summary>
public record BulkOperationStartedEvent(
    Guid JobId,
    string OperationType // e.g., "ConfigurationDeployment", "SoftwareUpdate"
) : INotification;