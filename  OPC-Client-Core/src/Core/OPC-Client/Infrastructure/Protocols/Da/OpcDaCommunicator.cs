using OPC.Client.Core.Application;
using OPC.Client.Core.Domain.ValueObjects;
using OPC.Client.Core.Exceptions;
using OPC.Client.Core.Infrastructure.Protocols.Common;
using OPC.Client.Core.Domain.Enums;
using OPC.Client.Core.Domain.DomainServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Opc.Da; // OPC Foundation DA .NET library
using Opc;     // OPC Foundation Common .NET library

namespace OPC.Client.Core.Infrastructure.Protocols.Da
{
    /// <summary>
    /// Implements OPC Data Access (DA) communication logic.
    /// REQ-CSVC-001, REQ-CSVC-002, REQ-CSVC-003, REQ-CSVC-004
    /// </summary>
    public class OpcDaCommunicator : IOpcProtocolClient
    {
        private readonly ILogger<OpcDaCommunicator> _logger;
        private readonly OpcVariantConverter _variantConverter;
        private Opc.Da.Server? _opcDaServer;
        private DaClientConfiguration? _daConfig;
        private string? _serverUrl;

        public OpcDaCommunicator(ILogger<OpcDaCommunicator> logger, OpcVariantConverter variantConverter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _variantConverter = variantConverter ?? throw new ArgumentNullException(nameof(variantConverter));
        }

        public void Configure(DaClientConfiguration? config)
        {
            _daConfig = config;
            if (_daConfig != null)
            {
                _serverUrl = Opc.Url.FormatUrl(Opc.Url.UriSchemeOpcDa, _daConfig.ServerHost ?? "localhost", _daConfig.ServerProgId ?? _daConfig.ServerClassId);
                _logger.LogInformation("OpcDaCommunicator configured for server URL: {ServerUrl}", _serverUrl);
            }
            else
            {
                 _logger.LogWarning("OpcDaCommunicator configured with null DaClientConfiguration.");
                 _serverUrl = null;
            }
        }

        public async Task ConnectAsync(ClientConfiguration clientConfig) // Main config passed, but uses its own _daConfig
        {
            if (_daConfig == null || string.IsNullOrEmpty(_serverUrl))
            {
                _logger.LogError("OpcDaCommunicator is not configured. Call Configure() with valid DaClientConfiguration first.");
                throw new InvalidOperationException("OpcDaCommunicator is not configured.");
            }

            _logger.LogInformation("Attempting to connect to OPC DA server: {ServerUrl}", _serverUrl);
            try
            {
                _opcDaServer = new Opc.Da.Server(); // No factory needed from SDK
                await Task.Run(() => _opcDaServer.Connect(new Opc.URL(_serverUrl), new ConnectData(new System.Net.NetworkCredential()))); // ConnectData can take credentials

                if (!_opcDaServer.IsConnected)
                {
                    throw new OpcCommunicationException($"Failed to connect to OPC DA server at {_serverUrl}. Server status: {_opcDaServer.GetStatus()?.ServerState}");
                }
                _logger.LogInformation("Successfully connected to OPC DA server: {ServerUrl}", _serverUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to OPC DA server: {ServerUrl}", _serverUrl);
                _opcDaServer?.Dispose();
                _opcDaServer = null;
                throw new OpcCommunicationException($"Connection to OPC DA server {_serverUrl} failed.", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_opcDaServer != null && _opcDaServer.IsConnected)
            {
                _logger.LogInformation("Disconnecting from OPC DA server: {ServerUrl}", _serverUrl);
                try
                {
                    await Task.Run(() => _opcDaServer.Disconnect());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during OPC DA server disconnect: {ServerUrl}", _serverUrl);
                    // Continue with dispose
                }
            }
            _opcDaServer?.Dispose();
            _opcDaServer = null;
            _logger.LogInformation("OPC DA client disconnected and disposed for {ServerUrl}.", _serverUrl);
        }

        public async Task<IEnumerable<NodeAddress>> BrowseNodesAsync(NodeAddress? nodeId)
        {
            CheckConnection();
            string browseStartItemId = nodeId?.Identifier ?? string.Empty; // Empty string for root
            _logger.LogDebug("Browsing OPC DA namespace from: '{BrowseStartItemId}' on {ServerUrl}", browseStartItemId, _serverUrl);

            try
            {
                BrowsePosition? position = null; // For continuation
                var browseFilters = new BrowseFilters() { BrowseFilter = browseFilter.all }; // Adjust filter as needed
                
                BrowseElement[] elements = await Task.Run(() => _opcDaServer!.Browse(new ItemIdentifier(browseStartItemId), browseFilters, out position));
                
                return elements?.Select(el => new NodeAddress(el.ItemPath != null ? el.ItemPath + el.Name : el.Name, null)).ToList() ?? Enumerable.Empty<NodeAddress>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing OPC DA namespace on {ServerUrl} from '{BrowseStartItemId}'", _serverUrl, browseStartItemId);
                throw new OpcCommunicationException($"Error browsing OPC DA namespace on {_serverUrl}.", ex);
            }
        }

        public async Task<IEnumerable<OpcDataValue>> ReadTagsAsync(IEnumerable<NodeAddress> tagAddresses)
        {
            CheckConnection();
            var itemsToRead = tagAddresses.Select(addr => new Opc.Da.Item { ItemID = addr.Identifier, ClientHandle = addr }).ToArray();
            if (!itemsToRead.Any()) return Enumerable.Empty<OpcDataValue>();

            _logger.LogDebug("Reading {ItemCount} tags from OPC DA server: {ServerUrl}", itemsToRead.Length, _serverUrl);

            try
            {
                ItemValueResult[]? results = await Task.Run(() => _opcDaServer!.Read(itemsToRead));
                
                return results?.Select(r => _variantConverter.ConvertDaItemValueToOpcDataValue(r, (NodeAddress)r.ClientHandle)).ToList()
                       ?? Enumerable.Empty<OpcDataValue>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading tags from OPC DA server: {ServerUrl}", _serverUrl);
                throw new OpcCommunicationException($"Error reading tags from OPC DA server {_serverUrl}.", ex);
            }
        }

        public async Task<bool> WriteTagsAsync(IEnumerable<OpcDataValue> tagValues)
        {
            CheckConnection();
            var itemsToWrite = tagValues.Select(val => new Opc.Da.ItemValue
            {
                ItemID = val.NodeAddress.Identifier,
                ClientHandle = val.NodeAddress,
                Value = _variantConverter.ConvertToGenericOpcValue(val.Value, OpcProtocolType.DA), // DA typically takes .NET types directly
                Quality = Qualities.Good, // Assume good quality for write, or map from OpcDataValue.Quality if needed
                Timestamp = val.Timestamp,
                TimestampSpecified = val.Timestamp != DateTime.MinValue
            }).ToArray();

            if (!itemsToWrite.Any()) return true;

            _logger.LogDebug("Writing {ItemCount} tags to OPC DA server: {ServerUrl}", itemsToWrite.Length, _serverUrl);

            try
            {
                IdentifiedResult[]? results = await Task.Run(() => _opcDaServer!.Write(itemsToWrite));
                
                bool allSucceeded = results?.All(r => r.ResultID.Succeeded()) ?? false;
                if (!allSucceeded && results != null)
                {
                    foreach (var result in results.Where(r => r.ResultID.Failed()))
                    {
                        _logger.LogWarning("Failed to write tag {ItemID} to OPC DA server {ServerUrl}. Error: {Error}", result.ItemName, _serverUrl, result.ResultID);
                    }
                }
                return allSucceeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing tags to OPC DA server: {ServerUrl}", _serverUrl);
                throw new OpcCommunicationException($"Error writing tags to OPC DA server {_serverUrl}.", ex);
            }
        }

        private void CheckConnection()
        {
            if (_opcDaServer == null || !_opcDaServer.IsConnected)
            {
                throw new OpcCommunicationException($"OpcDaCommunicator is not connected to server {_serverUrl}.");
            }
        }
    }
}