namespace OPC.Client.Core.Infrastructure.Protocols.Ua
{
    using Microsoft.Extensions.Logging;
    using Opc.Ua;
    using Opc.Ua.Client;
    using OPC.Client.Core.Domain.Aggregates.OpcUaSubscription;
    using OPC.Client.Core.Domain.Events;
    using OPC.Client.Core.Domain.ValueObjects;
    using OPC.Client.Core.Infrastructure.LocalDataBuffering;
    using OPC.Client.Core.Exceptions;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages the lifecycle and data handling for OPC UA subscriptions.
    /// Implements REQ-CSVC-023, REQ-CSVC-026.
    /// </summary>
    public class UaSubscriptionHandler : IDisposable
    {
        private readonly ILogger<UaSubscriptionHandler> _logger;
        private readonly ISubscriptionDataBuffer _dataBuffer;
        private readonly OPC.Client.Core.Domain.DomainServices.OpcVariantConverter _variantConverter;
        private Session? _session;
        private ApplicationConfiguration? _appConfiguration; // Needed for some SDK operations

        // Maps client-defined subscription ID (string) to SDK Subscription object
        private readonly ConcurrentDictionary<string, Subscription> _activeSdkSubscriptions = new ConcurrentDictionary<string, Subscription>();
        // Maps client-defined subscription ID (string) to the domain aggregate (for config and state)
        private readonly ConcurrentDictionary<string, OpcUaSubscription> _domainSubscriptions = new ConcurrentDictionary<string, OpcUaSubscription>();

        public event EventHandler<OpcDataChangedEvent>? DataChanged;
        public event EventHandler<string>? SubscriptionStatusChanged; // string is subscriptionId, payload could be more complex status DTO

        private readonly object _sessionLock = new object();
        private bool _isDisposed;

        public UaSubscriptionHandler(
            ILogger<UaSubscriptionHandler> logger,
            ISubscriptionDataBuffer dataBuffer,
            OPC.Client.Core.Domain.DomainServices.OpcVariantConverter variantConverter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataBuffer = dataBuffer ?? throw new ArgumentNullException(nameof(dataBuffer));
            _variantConverter = variantConverter ?? throw new ArgumentNullException(nameof(variantConverter));
        }

        public void Configure(Session session, ApplicationConfiguration appConfig)
        {
            lock (_sessionLock)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(UaSubscriptionHandler));

                _session = session ?? throw new ArgumentNullException(nameof(session));
                _appConfiguration = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
                _logger.LogInformation("UaSubscriptionHandler configured with SessionId: {SessionId}", _session.SessionId);

                // If there were pre-existing domain subscriptions (e.g. from config), attempt to activate them.
                // This scenario is complex if the handler is reconfigured with a new session.
                // Typically, subscriptions are created after the session is established.
                RestoreSubscriptionsAsync().ConfigureAwait(false).GetAwaiter().GetResult(); // Fire and forget for now
            }
        }

        /// <summary>
        /// Creates an OPC UA subscription based on the domain aggregate configuration.
        /// </summary>
        public async Task CreateSubscriptionAsync(OpcUaSubscription domainSubscription, string connectionId)
        {
            lock (_sessionLock)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(UaSubscriptionHandler));
                if (_session == null || !_session.Connected)
                    throw new OpcCommunicationException("Session is not connected for creating subscription.");
            }

            if (domainSubscription == null) throw new ArgumentNullException(nameof(domainSubscription));
            if (_activeSdkSubscriptions.ContainsKey(domainSubscription.Id))
            {
                _logger.LogWarning("Subscription with client ID {ClientSubscriptionId} already exists.", domainSubscription.Id);
                return; // Or throw
            }

            _logger.LogInformation("Creating OPC UA subscription: ClientId={ClientSubscriptionId}, PublishingInterval={Interval}ms",
                domainSubscription.Id, domainSubscription.PublishingIntervalMs);

            var sdkSubscription = new Subscription(_session.DefaultSubscription) // Use session's default as a template
            {
                ClientHandle = domainSubscription.Id, // Link SDK subscription to our ID
                DisplayName = $"Subscription_{domainSubscription.Id}",
                PublishingInterval = (int)domainSubscription.PublishingIntervalMs,
                LifetimeCount = domainSubscription.LifetimeCount,
                MaxKeepAliveCount = domainSubscription.MaxKeepAliveCount,
                MaxNotificationsPerPublish = domainSubscription.MaxNotificationsPerPublish,
                Priority = domainSubscription.Priority,
                PublishingEnabled = true, // Enable immediately
                TimestampsToReturn = TimestampsToReturn.Source // Or Both/Server based on preference
            };

            sdkSubscription.Notification += OnSdkSubscriptionNotification;
            // sdkSubscription.StatusChanged += OnSdkSubscriptionStatusChanged; // If SDK provides this

            List<MonitoredItemCreateRequest> monitoredItemRequests = new List<MonitoredItemCreateRequest>();
            foreach (var domainItem in domainSubscription.MonitoredItems)
            {
                var itemRequest = new MonitoredItemCreateRequest
                {
                    ItemToMonitor = new ReadValueId
                    {
                        NodeId = NodeId.Parse(domainItem.NodeAddress.Identifier, domainItem.NodeAddress.NamespaceIndex ?? 0),
                        AttributeId = Attributes.Value
                    },
                    MonitoringMode = MonitoringMode.Reporting, // Start reporting data
                    RequestedParameters = new MonitoringParameters
                    {
                        ClientHandle = Convert.ToUInt32(domainItem.ClientHandle), // SDK uses uint for client handle
                        SamplingInterval = domainItem.SamplingIntervalMs,
                        QueueSize = domainItem.QueueSize,
                        DiscardOldest = domainItem.DiscardOldest,
                        Filter = CreateDataChangeFilter(domainItem)
                    }
                };
                monitoredItemRequests.Add(itemRequest);
            }

            try
            {
                // Add to session (this makes the CreateSubscription call to server)
                _session.AddSubscription(sdkSubscription);
                // Create the subscription on the server
                await sdkSubscription.CreateAsync(null,null).ConfigureAwait(false); // Pass null for request/response header. SDK handles this.

                if (sdkSubscription.Created)
                {
                    _logger.LogInformation("SDK Subscription ClientId={ClientSubscriptionId} created successfully on server. ServerId={ServerSubscriptionId}",
                        domainSubscription.Id, sdkSubscription.Id);

                    if (monitoredItemRequests.Any())
                    {
                        // Add MonitoredItems to the SDK Subscription
                        var itemResults = await sdkSubscription.CreateMonitoredItemsAsync(null,null,TimestampsToReturn.Source, monitoredItemRequests).ConfigureAwait(false);

                        for (int i = 0; i < itemResults.Count; i++)
                        {
                            var domainItem = domainSubscription.MonitoredItems.FirstOrDefault(di => Convert.ToUInt32(di.ClientHandle) == monitoredItemRequests[i].RequestedParameters.ClientHandle);
                            if (StatusCode.IsGood(itemResults[i].StatusCode))
                            {
                                _logger.LogInformation("MonitoredItem for Node '{NodeId}' (ClientHandle={ClientHandle}) created successfully on subscription {ClientSubscriptionId}.",
                                    domainItem?.NodeAddress, domainItem?.ClientHandle, domainSubscription.Id);
                            }
                            else
                            {
                                _logger.LogError("Failed to create MonitoredItem for Node '{NodeId}' (ClientHandle={ClientHandle}) on subscription {ClientSubscriptionId}. StatusCode: {StatusCode}",
                                    domainItem?.NodeAddress, domainItem?.ClientHandle, domainSubscription.Id, itemResults[i].StatusCode);
                                // Handle partial failure: subscription might be created but items failed.
                            }
                        }
                    }

                    if (!_activeSdkSubscriptions.TryAdd(domainSubscription.Id, sdkSubscription) ||
                        !_domainSubscriptions.TryAdd(domainSubscription.Id, domainSubscription))
                    {
                        _logger.LogError("Failed to add subscription {ClientSubscriptionId} to internal tracking. Attempting cleanup.", domainSubscription.Id);
                        await RemoveSubscriptionAsync(domainSubscription.Id, connectionId).ConfigureAwait(false); // Attempt cleanup
                        throw new InvalidOperationException("Failed to track subscription internally.");
                    }
                    domainSubscription.UpdateState(OpcUaSubscription.SubscriptionState.Active);
                    SubscriptionStatusChanged?.Invoke(this, domainSubscription.Id);
                }
                else
                {
                    _logger.LogError("Failed to create SDK Subscription ClientId={ClientSubscriptionId} on server. StatusCode: {StatusCode}",
                        domainSubscription.Id, sdkSubscription.Status?.Error?.StatusCode);
                    sdkSubscription.Dispose(); // Clean up SDK object
                    domainSubscription.UpdateState(OpcUaSubscription.SubscriptionState.Error);
                    SubscriptionStatusChanged?.Invoke(this, domainSubscription.Id);
                    throw new OpcCommunicationException($"Failed to create subscription {domainSubscription.Id}. Server status: {sdkSubscription.Status?.Error?.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception creating subscription ClientId={ClientSubscriptionId}.", domainSubscription.Id);
                sdkSubscription?.Dispose();
                domainSubscription.UpdateState(OpcUaSubscription.SubscriptionState.Error);
                SubscriptionStatusChanged?.Invoke(this, domainSubscription.Id);
                throw new OpcCommunicationException($"Exception creating subscription {domainSubscription.Id}: {ex.Message}", ex);
            }
        }

        private ExtensionObject? CreateDataChangeFilter(Domain.Aggregates.OpcUaSubscription.MonitoredItem domainItem)
        {
            if (domainItem.DeadbandType.Equals("None", StringComparison.OrdinalIgnoreCase))
                return null;

            var filter = new DataChangeFilter
            {
                Trigger = Enum.TryParse<DataChangeTrigger>(domainItem.DataChangeFilter, true, out var trigger) ? trigger : DataChangeTrigger.StatusValue
            };

            if (domainItem.DeadbandType.Equals("Absolute", StringComparison.OrdinalIgnoreCase))
            {
                filter.DeadbandType = (uint)Opc.Ua.DeadbandType.Absolute;
                filter.DeadbandValue = domainItem.DeadbandValue;
            }
            else if (domainItem.DeadbandType.Equals("Percent", StringComparison.OrdinalIgnoreCase))
            {
                filter.DeadbandType = (uint)Opc.Ua.DeadbandType.Percent;
                filter.DeadbandValue = domainItem.DeadbandValue;
            }
            else
            {
                return null; // Unknown deadband type
            }
            return new ExtensionObject(filter);
        }


        public async Task RemoveSubscriptionAsync(string clientSubscriptionId, string connectionId)
        {
            if (string.IsNullOrEmpty(clientSubscriptionId)) return;

            _logger.LogInformation("Removing OPC UA subscription: ClientId={ClientSubscriptionId}", clientSubscriptionId);
            if (_activeSdkSubscriptions.TryRemove(clientSubscriptionId, out var sdkSubscription))
            {
                _domainSubscriptions.TryRemove(clientSubscriptionId, out var domainSub);
                sdkSubscription.Notification -= OnSdkSubscriptionNotification;
                // sdkSubscription.StatusChanged -= OnSdkSubscriptionStatusChanged;

                try
                {
                    if (_session != null && _session.Connected)
                    {
                        _session.RemoveSubscription(sdkSubscription); // Request server to delete
                        await sdkSubscription.DeleteAsync(true).ConfigureAwait(false); // Ensure it's deleted
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception removing SDK subscription ClientId={ClientSubscriptionId} from server.", clientSubscriptionId);
                    // Continue with local cleanup
                }
                finally
                {
                    sdkSubscription.Dispose();
                    _logger.LogInformation("SDK Subscription ClientId={ClientSubscriptionId} disposed.", clientSubscriptionId);
                    domainSub?.UpdateState(OpcUaSubscription.SubscriptionState.Deleted);
                    SubscriptionStatusChanged?.Invoke(this, clientSubscriptionId);
                }
            }
            else
            {
                _logger.LogWarning("Subscription ClientId={ClientSubscriptionId} not found for removal.", clientSubscriptionId);
            }
        }

        /// <summary>
        /// Handles notifications from an SDK subscription object.
        /// This is where data changes and events are received.
        /// </summary>
        private void OnSdkSubscriptionNotification(Subscription sdkSubscription, NotificationEventArgs e)
        {
            var clientSubscriptionId = sdkSubscription.ClientHandle as string;
            if (string.IsNullOrEmpty(clientSubscriptionId) || !_domainSubscriptions.TryGetValue(clientSubscriptionId, out var domainSub))
            {
                _logger.LogWarning("Received notification for unknown or untracked subscription. SDK ClientHandle: {ClientHandle}", sdkSubscription.ClientHandle);
                return;
            }

            try
            {
                if (e.NotificationMessage?.NotificationData == null)
                {
                    _logger.LogTrace("Received empty notification message for subscription {ClientSubscriptionId}.", clientSubscriptionId);
                    return;
                }

                List<OpcDataValue> changedValues = new List<OpcDataValue>();

                foreach (var notification in e.NotificationMessage.NotificationData)
                {
                    if (notification is DataChangeNotification dataChange)
                    {
                        _logger.LogDebug("Processing DataChangeNotification for subscription {ClientSubscriptionId} with {ItemCount} items.", clientSubscriptionId, dataChange.MonitoredItems.Count);
                        foreach (var monitoredItemNotification in dataChange.MonitoredItems)
                        {
                            var domainItem = domainSub.MonitoredItems.FirstOrDefault(item => Convert.ToUInt32(item.ClientHandle) == monitoredItemNotification.ClientHandle);
                            if (domainItem != null)
                            {
                                var opcDataValue = _variantConverter.ConvertUaDataValueToOpcDataValue(monitoredItemNotification.Value, domainItem.NodeAddress);
                                domainItem.UpdateLastValue(opcDataValue); // Update domain aggregate
                                changedValues.Add(opcDataValue);

                                _logger.LogTrace("Data changed for Node: {NodeId}, Value: {Value}, ClientHandle: {ClientHandle}",
                                    domainItem.NodeAddress, opcDataValue.Value, domainItem.ClientHandle);

                                // REQ-CSVC-026: Buffering during short network interruptions.
                                // The data buffer is used here. Publishing from buffer is handled upon reconnection.
                                _dataBuffer.AddData(clientSubscriptionId, opcDataValue);
                            }
                        }
                    }
                    else if (notification is EventNotificationList eventList)
                    {
                        _logger.LogDebug("Processing EventNotificationList for subscription {ClientSubscriptionId} with {EventCount} events.", clientSubscriptionId, eventList.Events.Count);
                        // TODO: Process events, map to domain AlarmEvent, and raise event.
                    }
                    else if (notification is StatusChangeNotification statusChange) // KeepAlive or status change
                    {
                         _logger.LogDebug("StatusChangeNotification for subscription {ClientSubscriptionId}. Status: {Status}, Sequence: {SeqNo}",
                             clientSubscriptionId, statusChange.Status, statusChange.SequenceNumber);
                         domainSub.HandleKeepAlive(statusChange.SequenceNumber, 0, 0); // Simplified, SDK handles counts.
                         if (StatusCode.IsBad(statusChange.Status))
                         {
                             domainSub.UpdateState(OpcUaSubscription.SubscriptionState.Error);
                             SubscriptionStatusChanged?.Invoke(this, clientSubscriptionId);
                         }
                    }
                }

                if (changedValues.Any())
                {
                    // Use connectionId from the domain subscription, assuming it's set.
                    // This requires the OpcUaSubscription aggregate to store its parent connectionId.
                    string connectionId = domainSub.ConnectionId;
                    var dataChangedEvent = new OpcDataChangedEvent(connectionId, clientSubscriptionId, changedValues);
                    DataChanged?.Invoke(this, dataChangedEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification for subscription {ClientSubscriptionId}", clientSubscriptionId);
            }
        }

        public void HandleSessionDisconnected()
        {
            _logger.LogWarning("Session disconnected. Updating subscription states to Disconnected.");
            foreach (var subId in _domainSubscriptions.Keys)
            {
                if (_domainSubscriptions.TryGetValue(subId, out var domainSub))
                {
                    domainSub.UpdateState(OpcUaSubscription.SubscriptionState.Disconnected);
                    SubscriptionStatusChanged?.Invoke(this, subId);
                }
            }
        }

        public async Task RestoreSubscriptionsAsync()
        {
            lock (_sessionLock)
            {
                if (_isDisposed || _session == null || !_session.Connected)
                {
                    _logger.LogWarning("Cannot restore subscriptions: Handler disposed or session not connected.");
                    return;
                }
            }

            _logger.LogInformation("Attempting to restore subscriptions after session (re)connection.");
            var clientIdsToRestore = _domainSubscriptions.Keys.ToList();

            foreach (var clientId in clientIdsToRestore)
            {
                if (_domainSubscriptions.TryGetValue(clientId, out var domainSub) &&
                    (domainSub.State == OpcUaSubscription.SubscriptionState.Disconnected ||
                     domainSub.State == OpcUaSubscription.SubscriptionState.Error ||
                     domainSub.State == OpcUaSubscription.SubscriptionState.Created)) // Also retry if initial creation failed
                {
                    _logger.LogInformation("Restoring subscription {ClientSubscriptionId}.", clientId);
                    try
                    {
                        // Remove any lingering SDK subscription first in case state is messy
                        if (_activeSdkSubscriptions.TryRemove(clientId, out var oldSdkSub))
                        {
                            oldSdkSub.Dispose();
                        }
                        await CreateSubscriptionAsync(domainSub, domainSub.ConnectionId).ConfigureAwait(false);

                        // After successful re-creation, process buffered data
                        var bufferedItems = _dataBuffer.GetAndClearBuffer(clientId);
                        if (bufferedItems.Any())
                        {
                             _logger.LogInformation("Processing {BufferedCount} buffered items for restored subscription {ClientSubscriptionId}", bufferedItems.Count, clientId);
                             var dataChangedEvent = new OpcDataChangedEvent(domainSub.ConnectionId, clientId, bufferedItems);
                             DataChanged?.Invoke(this, dataChangedEvent);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to restore subscription {ClientSubscriptionId}.", clientId);
                        domainSub.UpdateState(OpcUaSubscription.SubscriptionState.Error);
                        SubscriptionStatusChanged?.Invoke(this, clientId);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _logger.LogInformation("Disposing UaSubscriptionHandler.");
                var clientIds = _activeSdkSubscriptions.Keys.ToList();
                foreach (var id in clientIds)
                {
                    // Use a placeholder or find a way to get connectionId if needed for RemoveSubscriptionAsync
                    RemoveSubscriptionAsync(id, "disposing_connection_id").ConfigureAwait(false).GetAwaiter().GetResult();
                }
                _activeSdkSubscriptions.Clear();
                _domainSubscriptions.Clear();
                // _session is managed by OpcUaCommunicator
            }
            _isDisposed = true;
        }
    }
}