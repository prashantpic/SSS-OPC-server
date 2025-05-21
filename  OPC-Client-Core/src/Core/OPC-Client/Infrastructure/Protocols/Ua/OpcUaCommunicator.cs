using OPC.Client.Core.Application;
using OPC.Client.Core.Domain.ValueObjects;
using OPC.Client.Core.Exceptions;
using OPC.Client.Core.Infrastructure.Protocols.Common;
using OPC.Client.Core.Domain.Enums;
using OPC.Client.Core.Domain.DomainServices;
using OPC.Client.Core.Infrastructure.LocalDataBuffering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace OPC.Client.Core.Infrastructure.Protocols.Ua
{
    /// <summary>
    /// Implements OPC Unified Architecture (UA) communication logic.
    /// REQ-CSVC-001, REQ-CSVC-002, REQ-CSVC-003, REQ-CSVC-004, REQ-CSVC-023, REQ-3-001.
    /// </summary>
    public class OpcUaCommunicator : IOpcProtocolClient, IDisposable
    {
        private readonly ILogger<OpcUaCommunicator> _logger;
        private readonly OpcVariantConverter _variantConverter;
        private readonly UaSecurityHandler _securityHandler;
        public UaSubscriptionHandler UaSubscriptionHandler { get; } // Made public for Facade access
        private readonly ApplicationInstance _applicationInstance;
        private UaClientConfiguration? _uaConfig;
        private Session? _session;
        private bool _disposed = false;
        private string _serverUrl = string.Empty;

        public OpcUaCommunicator(
            ILogger<OpcUaCommunicator> logger,
            OpcVariantConverter variantConverter,
            UaSecurityHandler securityHandler,
            UaSubscriptionHandler subscriptionHandler,
            ISubscriptionDataBuffer dataBuffer) // dataBuffer is used by UaSubscriptionHandler
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _variantConverter = variantConverter ?? throw new ArgumentNullException(nameof(variantConverter));
            _securityHandler = securityHandler ?? throw new ArgumentNullException(nameof(securityHandler));
            UaSubscriptionHandler = subscriptionHandler ?? throw new ArgumentNullException(nameof(subscriptionHandler));

            _applicationInstance = new ApplicationInstance
            {
                ApplicationName = "OPC.Client.Core.UAClient",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "OPC.Client.Core.UAClient" // Ensure this section exists in app config if loading from file
            };
        }

        public void Configure(UaClientConfiguration? config)
        {
            _uaConfig = config;
            if (_uaConfig != null)
            {
                _logger.LogInformation("OpcUaCommunicator configured with UA settings. AppName: {AppName}", _uaConfig.ApplicationName);
                // Apply specific configurations to _applicationInstance or _securityHandler
                _applicationInstance.ApplicationName = _uaConfig.ApplicationName ?? _applicationInstance.ApplicationName;
                _securityHandler.Configure(_uaConfig.SecurityConfig);
                UaSubscriptionHandler.SetApplicationConfiguration(_applicationInstance.ApplicationConfiguration); // Pass app config to sub handler
            }
            else
            {
                 _logger.LogWarning("OpcUaCommunicator configured with null UaClientConfiguration.");
            }
        }

        public async Task ConnectAsync(ClientConfiguration clientConfig)
        {
            _serverUrl = clientConfig.ServerEndpoint;
            if (_uaConfig == null)
            {
                _logger.LogError("UA configuration (_uaConfig) is null. Call Configure() first.");
                throw new InvalidOperationException("UA specific configuration is not set for OpcUaCommunicator.");
            }

            try
            {
                // Load the application configuration.
                // This can be from a file or built programmatically.
                // Using a programmatic approach for simplicity here.
                var appConfiguration = await _applicationInstance.LoadApplicationConfiguration(silent: false);

                // Check the application certificate.
                bool haveAppCertificate = await _applicationInstance.CheckApplicationInstanceCertificate(silent: false, minimumKeySize: 0);
                if (!haveAppCertificate)
                {
                    _logger.LogError("Application instance certificate missing or invalid for OpcUaCommunicator.");
                    throw new OpcSecurityException("Application instance certificate is required and was not found or is invalid.");
                }
                appConfiguration.ApplicationUri = X509Utils.GetApplicationUriFromCertificate(appConfiguration.SecurityConfiguration.ApplicationCertificate.Certificate);

                // Pass the final app configuration to security handler if it needs it
                _securityHandler.SetApplicationConfiguration(appConfiguration);

                var selectedEndpoint = await _securityHandler.SelectEndpointAsync(_serverUrl, _uaConfig.SecurityConfig, appConfiguration);
                if (selectedEndpoint == null)
                {
                    throw new OpcCommunicationException($"No suitable endpoint found at {_serverUrl} for the configured security requirements.");
                }

                var endpointConfiguration = EndpointConfiguration.Create(appConfiguration);
                var configuredEndpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

                _logger.LogInformation("Attempting to create session with OPC UA server: {ServerUrl}", _serverUrl);
                _session = await Session.Create(
                    appConfiguration,
                    configuredEndpoint,
                    updateBeforeConnect: false, // If true, application configuration is updated with server certificate.
                    checkDomain: false, // Set to true if server certificate domain should be checked.
                    sessionName: _uaConfig.ApplicationName ?? "OPC.Client.Core Session",
                    sessionTimeout: _uaConfig.SessionTimeoutMs ?? 60000,
                    identity: _uaConfig.UserIdentity != null ? _securityHandler.GetUserIdentity(_uaConfig.UserIdentity) : new UserIdentity(new AnonymousIdentityToken()),
                    preferredLocales: null
                );

                _session.KeepAlive += Session_KeepAlive;
                UaSubscriptionHandler.SetSession(_session); // Pass session to subscription handler

                _logger.LogInformation("Successfully connected to OPC UA server: {ServerUrl}. Session ID: {SessionId}", _serverUrl, _session.SessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to OPC UA server: {ServerUrl}", _serverUrl);
                _session?.Dispose();
                _session = null;
                throw new OpcCommunicationException($"Connection to OPC UA server {_serverUrl} failed.", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_session != null)
            {
                _logger.LogInformation("Disconnecting from OPC UA server: {ServerUrl}, Session ID: {SessionId}", _serverUrl, _session.SessionId);
                try
                {
                    UaSubscriptionHandler.RemoveAllSubscriptions(); // Clean up subscriptions
                    await Task.Run(() => _session.Close());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during OPC UA session close: {ServerUrl}", _serverUrl);
                }
                _session.Dispose();
                _session = null;
            }
            _logger.LogInformation("OPC UA client disconnected and session disposed for {ServerUrl}.", _serverUrl);
        }

        public async Task<IEnumerable<NodeAddress>> BrowseNodesAsync(NodeAddress? nodeId)
        {
            CheckSession();
            NodeId browseStartNode = nodeId != null ? NodeId.Parse(nodeId.Identifier, nodeId.NamespaceIndex ?? 0) : ObjectIds.ObjectsFolder;
            _logger.LogDebug("Browsing OPC UA namespace from: {BrowseStartNode} on {ServerUrl}", browseStartNode, _serverUrl);

            try
            {
                BrowseDescription nodeToBrowse = new BrowseDescription
                {
                    NodeId = browseStartNode,
                    BrowseDirection = BrowseDirection.Forward,
                    ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences,
                    IncludeSubtypes = true,
                    NodeClassMask = 0, // All node classes
                    ResultMask = (uint)BrowseResultMask.All
                };

                ReferenceDescriptionCollection references = await Task.Run(() => ClientUtils.Browse(_session!, nodeToBrowse));
                
                return references.Select(rd => {
                    NodeId targetNodeId = ExpandedNodeId.ToNodeId(rd.NodeId, _session!.NamespaceUris);
                    return new NodeAddress(targetNodeId.Identifier.ToString(), targetNodeId.NamespaceIndex);
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing OPC UA namespace on {ServerUrl} from {BrowseStartNode}", _serverUrl, browseStartNode);
                throw new OpcCommunicationException($"Error browsing OPC UA namespace on {_serverUrl}.", ex);
            }
        }

        public async Task<IEnumerable<OpcDataValue>> ReadTagsAsync(IEnumerable<NodeAddress> tagAddresses)
        {
            CheckSession();
            var readValueIds = tagAddresses.Select(addr => new ReadValueId
            {
                NodeId = NodeId.Parse(addr.Identifier, addr.NamespaceIndex ?? 0),
                AttributeId = Attributes.Value
            }).ToList();

            if (!readValueIds.Any()) return Enumerable.Empty<OpcDataValue>();

            _logger.LogDebug("Reading {ItemCount} tags from OPC UA server: {ServerUrl}", readValueIds.Count, _serverUrl);

            try
            {
                _session!.Read(null, 0, TimestampsToReturn.Both, new ReadValueIdCollection(readValueIds), out DataValueCollection results, out DiagnosticInfoCollection diagnosticInfos);
                
                ClientBase.ValidateResponse(results, readValueIds); // Check for errors
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, readValueIds);

                var opcDataValues = new List<OpcDataValue>();
                for (int i = 0; i < results.Count; i++)
                {
                    opcDataValues.Add(_variantConverter.ConvertUaDataValueToOpcDataValue(results[i], tagAddresses.ElementAt(i)));
                }
                return opcDataValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading tags from OPC UA server: {ServerUrl}", _serverUrl);
                throw new OpcCommunicationException($"Error reading tags from OPC UA server {_serverUrl}.", ex);
            }
        }

        public async Task<bool> WriteTagsAsync(IEnumerable<OpcDataValue> tagValues)
        {
            CheckSession();
            var writeValues = new WriteValueCollection();
            foreach (var val in tagValues)
            {
                writeValues.Add(new WriteValue
                {
                    NodeId = NodeId.Parse(val.NodeAddress.Identifier, val.NodeAddress.NamespaceIndex ?? 0),
                    AttributeId = Attributes.Value,
                    Value = new DataValue(_variantConverter.ConvertToOpcUaVariant(val.Value), (StatusCode)val.Quality, val.Timestamp)
                });
            }

            if (!writeValues.Any()) return true;

            _logger.LogDebug("Writing {ItemCount} tags to OPC UA server: {ServerUrl}", writeValues.Count, _serverUrl);

            try
            {
                _session!.Write(null, writeValues, out StatusCodeCollection results, out DiagnosticInfoCollection diagnosticInfos);

                ClientBase.ValidateResponse(results, writeValues); // Check for errors
                ClientBase.ValidateDiagnosticInfos(diagnosticInfos, writeValues);
                
                bool allSucceeded = results.All(StatusCode.IsGood);
                if (!allSucceeded)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        if (StatusCode.IsBad(results[i]))
                        {
                            _logger.LogWarning("Failed to write tag {NodeId} to OPC UA server {ServerUrl}. Error: {Error}",
                                writeValues[i].NodeId, _serverUrl, results[i]);
                        }
                    }
                }
                return allSucceeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing tags to OPC UA server: {ServerUrl}", _serverUrl);
                throw new OpcCommunicationException($"Error writing tags to OPC UA server {_serverUrl}.", ex);
            }
        }

        private void Session_KeepAlive(Session session, KeepAliveEventArgs e)
        {
            if (ServiceResult.IsBad(e.Status))
            {
                _logger.LogWarning("Session KeepAlive to {ServerUrl} failed: {Status}. Attempting to reconnect.", _serverUrl, e.Status);
                // SDK handles reconnect attempts based on configuration.
                // UaSubscriptionHandler should be notified to manage subscription state.
                UaSubscriptionHandler.HandleSessionDisconnected();
            }
            else
            {
                _logger.LogTrace("Session KeepAlive successful for {ServerUrl}", _serverUrl);
            }
        }

        private void CheckSession()
        {
            if (_session == null || !_session.Connected)
            {
                throw new OpcCommunicationException($"OpcUaCommunicator is not connected to server {_serverUrl}.");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Disconnect and dispose the session
                if (_session != null)
                {
                    _session.KeepAlive -= Session_KeepAlive;
                    if (_session.Connected)
                    {
                        try { _session.Close(10000); } // Timeout for close
                        catch (Exception e) { _logger.LogWarning(e, "Exception closing session."); }
                    }
                    _session.Dispose();
                    _session = null;
                }
                // UaSubscriptionHandler might need explicit disposal if it holds unmanaged resources
                (UaSubscriptionHandler as IDisposable)?.Dispose();
            }
            _disposed = true;
        }
    }
}