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
using Opc.Hda; // OPC Foundation HDA .NET library
using Opc;     // OPC Foundation Common .NET library

namespace OPC.Client.Core.Infrastructure.Protocols.Hda
{
    /// <summary>
    /// Implements OPC Historical Data Access (HDA) communication logic.
    /// REQ-CSVC-001, REQ-CSVC-011, REQ-CSVC-012.
    /// </summary>
    public class OpcHdaCommunicator : IOpcProtocolClient
    {
        private readonly ILogger<OpcHdaCommunicator> _logger;
        private readonly OpcVariantConverter _variantConverter;
        private Opc.Hda.Server? _opcHdaServer;
        private DaClientConfiguration? _daConfig; // HDA often reuses DA config for server progid/host
        private string? _serverUrl;

        public OpcHdaCommunicator(ILogger<OpcHdaCommunicator> logger, OpcVariantConverter variantConverter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _variantConverter = variantConverter ?? throw new ArgumentNullException(nameof(variantConverter));
        }

        // HDA configuration might reuse DA server details
        public void Configure(DaClientConfiguration? config)
        {
            _daConfig = config;
            if (_daConfig != null)
            {
                _serverUrl = Opc.Url.FormatUrl(Opc.Url.UriSchemeOpcHda, _daConfig.ServerHost ?? "localhost", _daConfig.ServerProgId ?? _daConfig.ServerClassId);
                _logger.LogInformation("OpcHdaCommunicator configured for server URL: {ServerUrl}", _serverUrl);
            }
            else
            {
                _logger.LogWarning("OpcHdaCommunicator configured with null DaClientConfiguration for server details.");
                _serverUrl = null;
            }
        }

        public async Task ConnectAsync(ClientConfiguration clientConfig)
        {
             if (_daConfig == null || string.IsNullOrEmpty(_serverUrl))
            {
                _logger.LogError("OpcHdaCommunicator is not configured. Call Configure() with valid DaClientConfiguration for server details first.");
                throw new InvalidOperationException("OpcHdaCommunicator is not configured for server connection.");
            }

            _logger.LogInformation("Attempting to connect to OPC HDA server: {ServerUrl}", _serverUrl);
            try
            {
                _opcHdaServer = new Opc.Hda.Server();
                await Task.Run(() => _opcHdaServer.Connect(new Opc.URL(_serverUrl), new ConnectData(new System.Net.NetworkCredential())));

                if (!_opcHdaServer.IsConnected)
                {
                    throw new OpcCommunicationException($"Failed to connect to OPC HDA server at {_serverUrl}. Server status: {_opcHdaServer.GetServerStatus()?.ServerState}");
                }
                _logger.LogInformation("Successfully connected to OPC HDA server: {ServerUrl}", _serverUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to OPC HDA server: {ServerUrl}", _serverUrl);
                _opcHdaServer?.Dispose();
                _opcHdaServer = null;
                throw new OpcCommunicationException($"Connection to OPC HDA server {_serverUrl} failed.", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_opcHdaServer != null && _opcHdaServer.IsConnected)
            {
                _logger.LogInformation("Disconnecting from OPC HDA server: {ServerUrl}", _serverUrl);
                try
                {
                    await Task.Run(() => _opcHdaServer.Disconnect());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during OPC HDA server disconnect: {ServerUrl}", _serverUrl);
                }
            }
            _opcHdaServer?.Dispose();
            _opcHdaServer = null;
            _logger.LogInformation("OPC HDA client disconnected and disposed for {ServerUrl}.", _serverUrl);
        }

        public async Task<IEnumerable<NodeAddress>> BrowseNodesAsync(NodeAddress? nodeId)
        {
            CheckConnection();
            string browseStartItemId = nodeId?.Identifier ?? string.Empty;
            _logger.LogDebug("Browsing OPC HDA items from: '{BrowseStartItemId}' on {ServerUrl}", browseStartItemId, _serverUrl);

            try
            {
                // HDA Browsing is different. We can list trends (items) or attributes.
                // For IOpcProtocolClient compatibility, list items.
                BrowseElement[] elements = await Task.Run(() => _opcHdaServer!.Browse(
                    new ItemIdentifier(browseStartItemId),
                    BrowseDirection.All, // Or specific like Child, Sibling
                    "", // ElementNameFilter
                    "", // VendorFilter
                    null, // DataTypes
                    new BrowseFilter { BrowseFilter = Opc.Hda.browseFilter.item } // Filter for items/trends
                ));
                
                return elements?.Select(el => new NodeAddress(el.ItemPath != null ? el.ItemPath + el.Name : el.Name, null)).ToList() 
                       ?? Enumerable.Empty<NodeAddress>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing OPC HDA items on {ServerUrl} from '{BrowseStartItemId}'", _serverUrl, browseStartItemId);
                throw new OpcCommunicationException($"Error browsing OPC HDA items on {_serverUrl}.", ex);
            }
        }

        // ReadTagsAsync for HDA is not typical. HDA reads historical data.
        // This method could potentially read the *current* value of an HDA item if supported,
        // or throw ProtocolNotSupportedException. For now, not implemented.
        public Task<IEnumerable<OpcDataValue>> ReadTagsAsync(IEnumerable<NodeAddress> tagAddresses)
        {
            _logger.LogWarning("ReadTagsAsync (current values) is not typically supported by OPC HDA. Use QueryHistoricalDataAsync.");
            throw new ProtocolNotSupportedException("Reading current tag values via ReadTagsAsync is not standard for OPC HDA.");
        }

        // WriteTagsAsync for HDA is not supported. HDA is for historical data retrieval.
        public Task<bool> WriteTagsAsync(IEnumerable<OpcDataValue> tagValues)
        {
            _logger.LogWarning("WriteTagsAsync is not supported by OPC HDA.");
            throw new ProtocolNotSupportedException("Writing tags is not supported by OPC HDA.");
        }

        // HDA specific method (Not part of IOpcProtocolClient, would be called by Facade or specific HDA service)
        public async Task<IEnumerable<OpcDataValue>> QueryHistoricalDataAsync(
            IEnumerable<NodeAddress> tagAddresses,
            DateTime startTime,
            DateTime endTime,
            int maxValues,
            bool includeBounds,
            string? aggregationType = null, // e.g., "Average", "Min", "Max", "Interpolative"
            TimeSpan? resampleInterval = null)
        {
            CheckConnection();
            var itemIdentifiers = tagAddresses.Select(addr => new ItemIdentifier { ItemID = addr.Identifier }).ToArray();
            if (!itemIdentifiers.Any()) return Enumerable.Empty<OpcDataValue>();

            _logger.LogDebug("Querying historical data for {ItemCount} items from {StartTime} to {EndTime} on {ServerUrl}",
                itemIdentifiers.Length, startTime, endTime, _serverUrl);

            try
            {
                Trend trend = new Trend(_opcHdaServer!);
                trend.StartTime = startTime;
                trend.EndTime = endTime;
                trend.MaxValues = maxValues;
                trend.IncludeBounds = includeBounds;

                if (!string.IsNullOrEmpty(aggregationType) && Enum.TryParse<aggregate>(aggregationType, true, out var aggEnum))
                {
                    trend.Aggregate = aggEnum;
                    if (resampleInterval.HasValue)
                    {
                        trend.ResampleInterval = (decimal)resampleInterval.Value.TotalMilliseconds;
                    }
                } else if (!string.IsNullOrEmpty(aggregationType)) {
                    _logger.LogWarning("Unsupported HDA aggregation type: {AggregationType}. Reading raw data.", aggregationType);
                }


                ItemValueCollection[]? results = await Task.Run(() => trend.Read(itemIdentifiers));
                
                var historicalValues = new List<OpcDataValue>();
                if (results != null)
                {
                    for (int i = 0; i < results.Length; i++)
                    {
                        var itemResult = results[i];
                        var originalAddress = tagAddresses.ElementAt(i);
                        if (itemResult.ErrorResult != null && itemResult.ErrorResult.Failed())
                        {
                             _logger.LogWarning("Error reading historical data for tag {ItemID} from HDA: {Error}", itemResult.ItemName, itemResult.ErrorResult);
                             continue;
                        }
                        foreach(var val in itemResult)
                        {
                            historicalValues.Add(_variantConverter.ConvertHdaItemValueToOpcDataValue(val, originalAddress));
                        }
                    }
                }
                return historicalValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying historical data from OPC HDA server: {ServerUrl}", _serverUrl);
                throw new OpcCommunicationException($"Error querying historical data from OPC HDA server {_serverUrl}.", ex);
            }
        }


        private void CheckConnection()
        {
            if (_opcHdaServer == null || !_opcHdaServer.IsConnected)
            {
                throw new OpcCommunicationException($"OpcHdaCommunicator is not connected to server {_serverUrl}.");
            }
        }
    }
}