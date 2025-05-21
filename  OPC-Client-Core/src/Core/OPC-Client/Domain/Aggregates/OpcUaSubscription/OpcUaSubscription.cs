using OPC.Client.Core.Application.DTOs; // For UaSubscriptionConfigDto and MonitoredItemConfigDto
using OPC.Client.Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OPC.Client.Core.Domain.Aggregates
{
    /// <summary>
    /// Aggregate root representing an OPC UA subscription.
    /// Manages monitored items and data change notifications.
    /// REQ-CSVC-023, REQ-CSVC-026.
    /// </summary>
    public class OpcUaSubscription
    {
        public string Id { get; }
        public string ConnectionId { get; } // Links to OpcServerConnection
        public UaSubscriptionConfigDto Configuration { get; private set; }
        public SubscriptionStatus Status { get; private set; }
        public DateTime? LastStatusChangeTimestamp { get; private set; }
        public string? LastError { get; private set; }

        private readonly List<MonitoredItem> _monitoredItems = new List<MonitoredItem>();
        public IReadOnlyCollection<MonitoredItem> MonitoredItems => _monitoredItems.AsReadOnly();

        // Internal state for tracking server-side subscription ID, if different or needed
        public uint? ServerSubscriptionId { get; private set; }


        public OpcUaSubscription(string id, string connectionId, UaSubscriptionConfigDto configuration)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Status = SubscriptionStatus.Creating; // Initial status
            LastStatusChangeTimestamp = DateTime.UtcNow;

            // Initialize monitored items from configuration
            foreach (var itemConfig in configuration.MonitoredItems)
            {
                _monitoredItems.Add(new MonitoredItem(Guid.NewGuid().ToString(), itemConfig, this.Id));
            }
        }

        public void UpdateConfiguration(UaSubscriptionConfigDto newConfiguration)
        {
            Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
            // Potentially mark for modification with the server
            // This would involve removing/adding/modifying monitored items based on diff
        }

        public void AddMonitoredItem(MonitoredItemConfigDto itemConfig)
        {
            var existingItem = _monitoredItems.FirstOrDefault(mi => mi.Configuration.NodeAddress.Equals(itemConfig.NodeAddress));
            if (existingItem != null)
            {
                // Update existing item's configuration if needed, or throw
                // For simplicity, let's assume we disallow duplicate NodeAddress adds for now
                throw new InvalidOperationException($"Monitored item for NodeAddress '{itemConfig.NodeAddress}' already exists in subscription '{Id}'.");
            }
            var newItem = new MonitoredItem(Guid.NewGuid().ToString(), itemConfig, this.Id);
            _monitoredItems.Add(newItem);
            // Mark subscription for modification with the server
            // Raise domain event: MonitoredItemAddedToSubscriptionEvent
        }

        public void RemoveMonitoredItem(NodeAddress nodeAddress)
        {
            var itemToRemove = _monitoredItems.FirstOrDefault(mi => mi.Configuration.NodeAddress.Equals(nodeAddress));
            if (itemToRemove != null)
            {
                _monitoredItems.Remove(itemToRemove);
                // Mark subscription for modification with the server
                // Raise domain event: MonitoredItemRemovedFromSubscriptionEvent
            }
        }

        public void UpdateServerSubscriptionId(uint serverId)
        {
            ServerSubscriptionId = serverId;
        }

        /// <summary>
        /// Processes a data change notification for a specific monitored item.
        /// This might be called by an infrastructure handler.
        /// </summary>
        public void HandleDataChange(NodeAddress nodeAddress, OpcDataValue newValue)
        {
            var item = _monitoredItems.FirstOrDefault(mi => mi.Configuration.NodeAddress.Equals(nodeAddress));
            if (item != null)
            {
                item.UpdateLastValue(newValue);
                // Raise domain event: OpcDataChangedEvent (passing item details and newValue)
            }
            else
            {
                // Log warning: data change for unknown item
            }
        }


        public void SetStatus(SubscriptionStatus newStatus, string? errorMessage = null)
        {
            if (Status == newStatus && LastError == errorMessage) return;

            Status = newStatus;
            LastError = errorMessage;
            LastStatusChangeTimestamp = DateTime.UtcNow;

            // Raise a domain event: SubscriptionStatusChangedEvent(Id, newStatus, errorMessage)
        }
    }

    /// <summary>
    /// Represents a single monitored item within an OPC UA subscription.
    /// This is an Entity within the OpcUaSubscription Aggregate.
    /// </summary>
    public class MonitoredItem
    {
        public string Id { get; } // Internal unique ID for this monitored item
        public string SubscriptionId { get; } // Parent subscription ID
        public MonitoredItemConfigDto Configuration { get; private set; }
        public OpcDataValue? LastValue { get; private set; }
        public uint? ServerHandle { get; set; } // Handle assigned by the server for this item

        public MonitoredItem(string id, MonitoredItemConfigDto configuration, string subscriptionId)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            SubscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void UpdateConfiguration(MonitoredItemConfigDto newConfiguration)
        {
            Configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
            // Mark for modification with the server
        }

        internal void UpdateLastValue(OpcDataValue newValue)
        {
            LastValue = newValue;
        }
    }

    public enum SubscriptionStatus
    {
        Creating,   // Being created on the client/server
        Active,     // Actively receiving data
        Error,      // An error occurred
        Deleting,   // Being deleted
        Deleted,    // Successfully deleted
        Disabled,   // Temporarily disabled by user or system
        Reconnecting // Attempting to recover after connection loss
    }
}