using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Opc.System.Services.Management.Application.Shared;
using Opc.System.Services.Management.Domain.Aggregates;
using Opc.System.Services.Management.Domain.Repositories;

namespace Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration
{
    /// <summary>
    /// This handler processes a command to update a client's configuration.
    /// It retrieves the client aggregate, invokes its business logic, and saves the result.
    /// </summary>
    public class UpdateClientConfigurationCommandHandler : IRequestHandler<UpdateClientConfigurationCommand, Result>
    {
        private readonly IOpcClientInstanceRepository _clientInstanceRepository;
        
        // Assuming a UnitOfWork pattern would be used in a full-fledged application
        // to commit changes after the handler completes. For now, the repository handles saves.
        // private readonly IUnitOfWork _unitOfWork;

        public UpdateClientConfigurationCommandHandler(IOpcClientInstanceRepository clientInstanceRepository)
        {
            _clientInstanceRepository = clientInstanceRepository;
        }

        /// <summary>
        /// Handles the logic for the UpdateClientConfigurationCommand.
        /// </summary>
        /// <param name="command">The command containing the client ID and new configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Result indicating success or failure.</returns>
        public async Task<Result> Handle(UpdateClientConfigurationCommand command, CancellationToken cancellationToken)
        {
            var clientInstanceId = new ClientInstanceId(command.ClientInstanceId);
            var clientInstance = await _clientInstanceRepository.GetByIdAsync(clientInstanceId, cancellationToken);

            if (clientInstance is null)
            {
                return Result.Failure($"Client instance with ID '{command.ClientInstanceId}' not found.");
            }

            // Map DTO to Domain Object
            var newConfig = new ClientConfiguration(
                command.NewConfiguration.TagConfigurations.Select(t => new TagConfiguration(t.TagName, t.NodeId, t.DataType)).ToList(),
                command.NewConfiguration.ServerConnections.Select(s => new ServerConnectionSetting(s.Name, s.EndpointUrl)).ToList(),
                TimeSpan.FromSeconds(command.NewConfiguration.PollingIntervalInSeconds)
            );

            clientInstance.UpdateConfiguration(newConfig);

            await _clientInstanceRepository.UpdateAsync(clientInstance, cancellationToken);
            
            // await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}