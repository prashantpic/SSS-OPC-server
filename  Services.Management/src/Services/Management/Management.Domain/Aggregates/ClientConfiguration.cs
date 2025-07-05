using System;
using System.Collections.Generic;

namespace Opc.System.Services.Management.Domain.Aggregates
{
    /// <summary>
    /// A data structure that encapsulates all settings for an OPC client,
    /// such as server connection details and tag lists. This is a complex value object owned by OpcClientInstance.
    /// </summary>
    public record ClientConfiguration
    {
        /// <summary>
        /// List of tag settings.
        /// </summary>
        public IReadOnlyCollection<TagConfiguration> TagConfigurations { get; init; }

        /// <summary>
        /// OPC Server connection details.
        /// </summary>
        public IReadOnlyCollection<ServerConnectionSetting> ServerConnections { get; init; }

        /// <summary>
        /// The client's main polling interval.
        /// </summary>
        public TimeSpan PollingInterval { get; init; }

        public ClientConfiguration(IReadOnlyCollection<TagConfiguration> tagConfigurations, IReadOnlyCollection<ServerConnectionSetting> serverConnections, TimeSpan pollingInterval)
        {
            TagConfigurations = tagConfigurations ?? new List<TagConfiguration>();
            ServerConnections = serverConnections ?? new List<ServerConnectionSetting>();
            PollingInterval = pollingInterval;

            if (pollingInterval <= TimeSpan.Zero)
            {
                throw new ArgumentException("Polling interval must be positive.", nameof(pollingInterval));
            }
        }
    }

    /// <summary>
    /// Placeholder record for OPC tag settings.
    /// </summary>
    public record TagConfiguration(string TagName, string NodeId, string DataType);
    
    /// <summary>
    /// Placeholder record for OPC server connection settings.
    /// </summary>
    public record ServerConnectionSetting(string Name, string EndpointUrl);
}