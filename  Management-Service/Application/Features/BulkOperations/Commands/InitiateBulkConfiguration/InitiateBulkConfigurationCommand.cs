using MediatR;
using System;
using System.Collections.Generic;

namespace ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkConfiguration;

// MediatR command representing a request to initiate bulk configuration deployment.
public record InitiateBulkConfigurationCommand(
    List<Guid> ClientInstanceIds,
    Guid ConfigurationVersionId
) : IRequest<Guid>; // Returns the ID of the created BulkOperationJob