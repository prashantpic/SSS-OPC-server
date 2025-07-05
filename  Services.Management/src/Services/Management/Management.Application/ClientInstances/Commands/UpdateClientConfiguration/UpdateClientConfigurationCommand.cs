using System;
using System.Collections.Generic;
using MediatR;
using Opc.System.Services.Management.Application.Shared;

namespace Opc.System.Services.Management.Application.ClientInstances.Commands.UpdateClientConfiguration
{
    /// <summary>
    /// A command DTO that holds the ID of the client to be updated and its new configuration settings.
    /// This encapsulates the intent and data required to update an OPC client's configuration.
    /// </summary>
    public record UpdateClientConfigurationCommand(
        Guid ClientInstanceId,
        ClientConfigurationDto NewConfiguration) : IRequest<Result>;

    /// <summary>
    /// Data Transfer Object for client configuration.
    /// </summary>
    public record ClientConfigurationDto(
        IReadOnlyCollection<TagConfigurationDto> TagConfigurations,
        IReadOnlyCollection<ServerConnectionSettingDto> ServerConnections,
        int PollingIntervalInSeconds);

    /// <summary>
    /// DTO for tag configuration.
    /// </summary>
    public record TagConfigurationDto(string TagName, string NodeId, string DataType);

    /// <summary>
    /// DTO for server connection settings.
    /// </summary>
    public record ServerConnectionSettingDto(string Name, string EndpointUrl);
}