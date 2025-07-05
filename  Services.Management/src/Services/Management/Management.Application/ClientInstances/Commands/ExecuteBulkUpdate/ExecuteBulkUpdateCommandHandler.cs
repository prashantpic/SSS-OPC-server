using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Opc.System.Services.Management.Domain.Aggregates;
using Opc.System.Services.Management.Domain.Repositories;

namespace Opc.System.Services.Management.Application.ClientInstances.Commands.ExecuteBulkUpdate
{
    /// <summary>
    /// Processes a bulk update command, applying a new configuration to a list of client aggregates and saving the changes.
    /// </summary>
    public class ExecuteBulkUpdateCommandHandler : IRequestHandler<ExecuteBulkUpdateCommand, BulkUpdateResultDto>
    {
        private readonly IOpcClientInstanceRepository _clientInstanceRepository;

        public ExecuteBulkUpdateCommandHandler(IOpcClientInstanceRepository clientInstanceRepository)
        {
            _clientInstanceRepository = clientInstanceRepository;
        }

        /// <summary>
        /// Handles the logic for applying a configuration to multiple clients.
        /// </summary>
        /// <param name="command">The command with client IDs and the configuration to apply.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A DTO summarizing the successful and failed updates.</returns>
        public async Task<BulkUpdateResultDto> Handle(ExecuteBulkUpdateCommand command, CancellationToken cancellationToken)
        {
            var clientInstanceIds = command.ClientInstanceIds.Select(id => new ClientInstanceId(id)).ToList();
            var clientInstances = await _clientInstanceRepository.GetByIdsAsync(clientInstanceIds, cancellationToken);

            var successfulIds = new List<Guid>();
            var failedIds = new List<Guid>();

            // Map DTO to Domain Object once
            var configToApply = new ClientConfiguration(
                command.ConfigurationToApply.TagConfigurations.Select(t => new TagConfiguration(t.TagName, t.NodeId, t.DataType)).ToList(),
                command.ConfigurationToApply.ServerConnections.Select(s => new ServerConnectionSetting(s.Name, s.EndpointUrl)).ToList(),
                TimeSpan.FromSeconds(command.ConfigurationToApply.PollingIntervalInSeconds)
            );
            
            var foundInstanceIds = new HashSet<Guid>();

            foreach (var instance in clientInstances)
            {
                instance.UpdateConfiguration(configToApply);
                await _clientInstanceRepository.UpdateAsync(instance, cancellationToken);
                successfulIds.Add(instance.Id.Value);
                foundInstanceIds.Add(instance.Id.Value);
            }

            // Identify which requested IDs were not found in the repository
            failedIds.AddRange(command.ClientInstanceIds.Where(id => !foundInstanceIds.Contains(id)));

            return new BulkUpdateResultDto(successfulIds, failedIds);
        }
    }
}