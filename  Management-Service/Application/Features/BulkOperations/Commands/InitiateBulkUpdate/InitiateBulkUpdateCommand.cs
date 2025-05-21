using MediatR;
using System;
using System.Collections.Generic;

namespace ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkUpdate;

// MediatR command representing a request to initiate bulk software updates for clients.
public record InitiateBulkUpdateCommand(
    List<Guid> ClientInstanceIds,
    string UpdatePackageUrl,
    string TargetVersion
) : IRequest<Guid>; // Returns the ID of the created BulkOperationJob