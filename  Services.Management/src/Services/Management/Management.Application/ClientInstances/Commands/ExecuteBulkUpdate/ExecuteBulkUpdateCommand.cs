using System;
using System.Collections.Generic;
using MediatR;
using Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration;

namespace Opc.System.Services.Management.Application.ClientInstances.Commands.ExecuteBulkUpdate
{
    /// <summary>
    /// A command that triggers the application of a configuration to a specified list of client instances.
    /// </summary>
    public record ExecuteBulkUpdateCommand(
        IEnumerable<Guid> ClientInstanceIds,
        ClientConfigurationDto ConfigurationToApply) : IRequest<BulkUpdateResultDto>;

    /// <summary>
    /// DTO representing the result of a bulk update operation.
    /// </summary>
    public record BulkUpdateResultDto(
        IEnumerable<Guid> SuccessfulIds,
        IEnumerable<Guid> FailedIds);
}