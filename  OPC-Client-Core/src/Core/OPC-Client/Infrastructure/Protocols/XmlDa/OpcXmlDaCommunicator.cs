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
using Opc.Xml.Da.Service; // Using OPC Foundation XML-DA generated service client

namespace OPC.Client.Core.Infrastructure.Protocols.XmlDa
{
    /// <summary>
    /// Implements OPC XML-DA v1.01 communication logic.
    /// REQ-CSVC-001
    /// </summary>
    public class OpcXmlDaCommunicator : IOpcProtocolClient
    {
        private readonly ILogger<OpcXmlDaCommunicator> _logger;
        private readonly OpcVariantConverter _variantConverter;
        private OPCXML_DAService? _client; // The generated SOAP client
        private string _endpointUrl = string.Empty;

        public OpcXmlDaCommunicator(ILogger<OpcXmlDaCommunicator> logger, OpcVariantConverter variantConverter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _variantConverter = variantConverter ?? throw new ArgumentNullException(nameof(variantConverter));
        }

        public void Configure(string endpointUrl)
        {
            _endpointUrl = endpointUrl;
            _logger.LogInformation("OpcXmlDaCommunicator configured for endpoint: {EndpointUrl}", _endpointUrl);
        }

        public async Task ConnectAsync(ClientConfiguration clientConfig)
        {
            _endpointUrl = clientConfig.ServerEndpoint; // XML-DA endpoint comes from main config
            if (string.IsNullOrEmpty(_endpointUrl))
            {
                _logger.LogError("OPC XML-DA endpoint URL is not configured.");
                throw new InvalidOperationException("OPC XML-DA endpoint URL is not configured.");
            }

            _logger.LogInformation("Attempting to 'connect' (initialize client proxy) to OPC XML-DA server: {EndpointUrl}", _endpointUrl);
            try
            {
                _client = new OPCXML_DAService { Url = _endpointUrl };
                // Optionally configure timeouts, credentials, client certificates here
                // _client.Timeout = clientConfig.DefaultOperationTimeoutMs ?? 60000;

                // Verify connection by making a GetStatus call
                var statusRequest = new GetStatus();
                var statusResponse = await Task.Run(() => _client.GetStatus(statusRequest)); // XML-DA SDK methods are sync

                if (statusResponse?.GetStatusResult == null || !statusResponse.GetStatusResult.ServerStateSpecified)
                {
                    throw new OpcCommunicationException($"Failed to get valid status from OPC XML-DA server at {_endpointUrl}.");
                }
                _logger.LogInformation("Successfully 'connected' to OPC XML-DA server: {EndpointUrl}. Server State: {ServerState}",
                                       _endpointUrl, statusResponse.GetStatusResult.ServerState);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to OPC XML-DA server: {EndpointUrl}", _endpointUrl);
                _client?.Dispose();
                _client = null;
                throw new OpcCommunicationException($"Connection to OPC XML-DA server {_endpointUrl} failed.", ex);
            }
        }

        public Task DisconnectAsync()
        {
            _logger.LogInformation("Disconnecting (disposing client proxy) from OPC XML-DA server: {EndpointUrl}", _endpointUrl);
            _client?.Dispose();
            _client = null;
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<NodeAddress>> BrowseNodesAsync(NodeAddress? nodeId)
        {
            CheckClient();
            string browseStartItemId = nodeId?.Identifier ?? string.Empty; // Root if empty
            _logger.LogDebug("Browsing OPC XML-DA items from: '{BrowseStartItemId}' on {EndpointUrl}", browseStartItemId, _endpointUrl);

            try
            {
                var request = new Browse
                {
                    ItemID = browseStartItemId,
                    ItemPath = null, // Specify path if browsing within an item
                    BrowseFilter = BrowseFilter.all, // Or branches, items
                    ReturnAllProperties = false,
                    ReturnPropertyValues = false,
                    ReturnErrorText = true
                };
                var response = await Task.Run(() => _client!.Browse(request));

                if (response.BrowseResult?.Errors != null && response.BrowseResult.Errors.Any())
                {
                    var error = response.BrowseResult.ErrorsFirst;
                    throw new OpcCommunicationException($"Error browsing OPC XML-DA: {error.Text} (Code: {error.ID})");
                }
                
                return response.BrowseResult?.Elements?.Select(el => new NodeAddress(el.ItemID, null)).ToList() 
                       ?? Enumerable.Empty<NodeAddress>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing OPC XML-DA items on {EndpointUrl} from '{BrowseStartItemId}'", _endpointUrl, browseStartItemId);
                throw new OpcCommunicationException($"Error browsing OPC XML-DA items on {_endpointUrl}.", ex);
            }
        }

        public async Task<IEnumerable<OpcDataValue>> ReadTagsAsync(IEnumerable<NodeAddress> tagAddresses)
        {
            CheckClient();
            var requestItems = tagAddresses.Select(addr => new ReadRequestItem { ItemID = addr.Identifier }).ToArray();
            if (!requestItems.Any()) return Enumerable.Empty<OpcDataValue>();

            _logger.LogDebug("Reading {ItemCount} tags from OPC XML-DA server: {EndpointUrl}", requestItems.Length, _endpointUrl);

            try
            {
                var request = new Read
                {
                    ReturnErrorText = true,
                    ItemList = new ReadRequestItemList { Items = requestItems }
                    // Options like MaxAge can be set here if needed
                };
                var response = await Task.Run(() => _client!.Read(request));

                if (response.ReadResult?.Errors != null && response.ReadResult.Errors.Any())
                {
                     _logger.LogWarning("Errors occurred during XML-DA read operation. First error: {ErrorText}", response.ReadResult.ErrorsFirst.Text);
                     // Individual item errors are in response.ReadResult.Items[i].Error
                }

                var opcDataValues = new List<OpcDataValue>();
                if (response.ReadResult?.Items != null)
                {
                    foreach (var itemValue in response.ReadResult.Items)
                    {
                        var originalAddress = tagAddresses.FirstOrDefault(a => a.Identifier == itemValue.ItemID);
                        if (originalAddress == null)
                        {
                            _logger.LogWarning("Received XML-DA read result for an unrequested ItemID: {ItemID}", itemValue.ItemID);
                            continue;
                        }

                        if (itemValue.Error != null)
                        {
                             _logger.LogWarning("Error reading tag {ItemID} from XML-DA: {ErrorText}", itemValue.ItemID, itemValue.Error.Text);
                             opcDataValues.Add(new OpcDataValue(originalAddress, null, itemValue.Error.ID.ToString(), DateTime.UtcNow)); // Use error code as quality
                             continue;
                        }
                        
                        opcDataValues.Add(new OpcDataValue(
                            originalAddress,
                            _variantConverter.ConvertFromGenericOpcValue(itemValue.Value, OpcProtocolType.XmlDA),
                            itemValue.Quality?.QualityField.ToString() ?? "Unknown", // XML-DA QualityField is an int
                            itemValue.TimestampSpecified ? itemValue.Timestamp : DateTime.MinValue
                        ));
                    }
                }
                return opcDataValues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading tags from OPC XML-DA server: {EndpointUrl}", _endpointUrl);
                throw new OpcCommunicationException($"Error reading tags from OPC XML-DA server {_endpointUrl}.", ex);
            }
        }

        public async Task<bool> WriteTagsAsync(IEnumerable<OpcDataValue> tagValues)
        {
            CheckClient();
            var writeRequestItems = tagValues.Select(val => new WriteRequestItem
            {
                ItemID = val.NodeAddress.Identifier,
                Value = _variantConverter.ConvertToGenericOpcValue(val.Value, OpcProtocolType.XmlDA), // XML-DA usually takes object directly
                Timestamp = val.Timestamp,
                TimestampSpecified = val.Timestamp != DateTime.MinValue
            }).ToArray();

            if (!writeRequestItems.Any()) return true;

            _logger.LogDebug("Writing {ItemCount} tags to OPC XML-DA server: {EndpointUrl}", writeRequestItems.Length, _endpointUrl);

            try
            {
                var request = new Write
                {
                    ReturnErrorText = true,
                    ItemList = new WriteRequestItemList { Items = writeRequestItems }
                };
                var response = await Task.Run(() => _client!.Write(request));

                bool allSucceeded = true;
                if (response.WriteResult?.Errors != null && response.WriteResult.Errors.Any())
                {
                    allSucceeded = false; // General errors occurred
                    foreach(var error in response.WriteResult.Errors)
                    {
                        _logger.LogWarning("Failed to write to OPC XML-DA server {EndpointUrl}. Error for item {ItemID}: {ErrorText}", _endpointUrl, error.ItemID ?? "General", error.Text);
                    }
                }
                // XML-DA WriteResult often does not provide per-item success, only errors.
                // If no errors, assume success.
                return allSucceeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing tags to OPC XML-DA server: {EndpointUrl}", _endpointUrl);
                throw new OpcCommunicationException($"Error writing tags to OPC XML-DA server {_endpointUrl}.", ex);
            }
        }

        private void CheckClient()
        {
            if (_client == null)
            {
                throw new OpcCommunicationException($"OpcXmlDaCommunicator is not connected (client proxy not initialized) for endpoint {_endpointUrl}.");
            }
        }
    }
}