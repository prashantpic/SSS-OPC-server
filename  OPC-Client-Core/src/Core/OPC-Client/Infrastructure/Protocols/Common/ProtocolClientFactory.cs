using OPC.Client.Core.Application;
using OPC.Client.Core.Domain.Enums;
using OPC.Client.Core.Exceptions;
using OPC.Client.Core.Infrastructure.Protocols.Da;
using OPC.Client.Core.Infrastructure.Protocols.Ua;
using OPC.Client.Core.Infrastructure.Protocols.XmlDa;
using OPC.Client.Core.Infrastructure.Protocols.Hda;
using OPC.Client.Core.Infrastructure.Protocols.Ac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection; // Required for IServiceProvider
using System;

namespace OPC.Client.Core.Infrastructure.Protocols.Common
{
    /// <summary>
    /// Factory class for creating instances of specific OPC protocol clients based on configuration.
    /// Responsible for creating the appropriate IOpcProtocolClient implementation.
    /// REQ-CSVC-001
    /// </summary>
    public class ProtocolClientFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProtocolClientFactory> _logger;

        public ProtocolClientFactory(IServiceProvider serviceProvider, ILogger<ProtocolClientFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates an instance of the appropriate OPC protocol client based on the provided configuration.
        /// </summary>
        /// <param name="config">The client configuration containing the protocol type and connection details.</param>
        /// <returns>An instance of <see cref="IOpcProtocolClient"/>.</returns>
        public IOpcProtocolClient CreateClient(ClientConfiguration config)
        {
            if (config == null)
            {
                _logger.LogError("ClientConfiguration cannot be null for ProtocolClientFactory.CreateClient.");
                throw new ArgumentNullException(nameof(config));
            }

            _logger.LogInformation("Creating OPC protocol client for Protocol: {ProtocolType}, Endpoint: {ServerEndpoint}",
                config.ProtocolType, config.ServerEndpoint);

            // This factory assumes that the specific communicator classes (OpcUaCommunicator, OpcDaCommunicator, etc.)
            // are registered with the IServiceProvider.
            try
            {
                switch (config.ProtocolType)
                {
                    case OpcProtocolType.UA:
                        var uaCommunicator = _serviceProvider.GetRequiredService<OpcUaCommunicator>();
                        uaCommunicator.Configure(config.UaConfig); // Pass UA specific config
                        return uaCommunicator;

                    case OpcProtocolType.DA:
                        var daCommunicator = _serviceProvider.GetRequiredService<OpcDaCommunicator>();
                        daCommunicator.Configure(config.DaConfig); // Pass DA specific config
                        return daCommunicator;

                    case OpcProtocolType.XmlDA:
                        var xmlDaCommunicator = _serviceProvider.GetRequiredService<OpcXmlDaCommunicator>();
                        xmlDaCommunicator.Configure(config.ServerEndpoint); // XML-DA might just need endpoint URL
                        return xmlDaCommunicator;

                    case OpcProtocolType.HDA:
                        var hdaCommunicator = _serviceProvider.GetRequiredService<OpcHdaCommunicator>();
                        hdaCommunicator.Configure(config.DaConfig); // HDA might re-use DA config for server details
                        return hdaCommunicator;

                    case OpcProtocolType.AC:
                        var acCommunicator = _serviceProvider.GetRequiredService<OpcAcCommunicator>();
                        acCommunicator.Configure(config.DaConfig); // A&C might re-use DA config for server details
                        return acCommunicator;

                    default:
                        _logger.LogError("Unsupported OPC protocol type specified: {ProtocolType}", config.ProtocolType);
                        throw new ProtocolNotSupportedException($"OPC protocol type '{config.ProtocolType}' is not supported.");
                }
            }
            catch (InvalidOperationException ex) // Catches GetRequiredService failures
            {
                _logger.LogError(ex, "Failed to resolve OPC communicator for protocol {ProtocolType}. Ensure it's registered in DI.", config.ProtocolType);
                throw new OpcConfigurationException($"Failed to create OPC client for protocol {config.ProtocolType}. Service not registered or misconfigured.", ex);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred while creating OPC client for protocol {ProtocolType}.", config.ProtocolType);
                throw; // Re-throw other unexpected exceptions
            }
        }
    }
}