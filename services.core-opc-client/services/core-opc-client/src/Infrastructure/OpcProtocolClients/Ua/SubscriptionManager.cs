using Opc.Ua;
using Opc.Ua.Client;
using services.opc.client.Domain.Models;

namespace services.opc.client.Infrastructure.OpcProtocolClients.Ua;

/// <summary>
/// Manages OPC UA subscriptions for a given session. It handles the creation of subscriptions,
/// adding monitored items, and processing data change notifications from the server.
/// </summary>
public class SubscriptionManager
{
    private readonly Session _session;
    private readonly ILogger _logger;
    private readonly List<Subscription> _subscriptions = new();

    /// <summary>
    /// Event fired when a data change notification is received and processed.
    /// </summary>
    public event Action<DataPoint>? OnDataReceived;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionManager"/> class.
    /// </summary>
    /// <param name="session">The active OPC UA session.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    public SubscriptionManager(Session session, ILogger logger)
    {
        _session = session;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new subscription on the server based on the provided settings.
    /// </summary>
    /// <param name="settings">The settings for the subscription and its monitored items.</param>
    public void CreateSubscription(SubscriptionSettings settings)
    {
        try
        {
            var subscription = new Subscription(_session.DefaultSubscription)
            {
                PublishingInterval = settings.PublishingInterval,
                KeepAliveCount = 10,
                LifetimeCount = 30,
                MaxNotificationsPerPublish = 1000,
                TimestampsToReturn = TimestampsToReturn.Both,
            };

            var items = new List<MonitoredItem>();
            foreach (var tag in settings.Tags)
            {
                var item = new MonitoredItem(subscription.DefaultItem)
                {
                    DisplayName = tag,
                    StartNodeId = tag,
                    AttributeId = Attributes.Value,
                    SamplingInterval = settings.SamplingInterval,
                    QueueSize = 1,
                    DiscardOldest = true
                };
                item.Notification += OnNotification;
                items.Add(item);
            }
            
            subscription.AddItems(items);
            _session.AddSubscription(subscription);
            subscription.Create();

            _subscriptions.Add(subscription);
            _logger.LogInformation("Successfully created subscription with {ItemCount} items. Publishing Interval: {Interval}ms", items.Count, settings.PublishingInterval);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to create OPC UA subscription.");
            // Allow the application to continue without this subscription
        }
    }
    
    /// <summary>
    /// Deletes all managed subscriptions from the server.
    /// </summary>
    public void DeleteAllSubscriptions()
    {
        try
        {
            if (_subscriptions.Any())
            {
                _session.RemoveSubscriptions(_subscriptions);
                _subscriptions.Clear();
                _logger.LogInformation("All subscriptions removed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting subscriptions.");
        }
    }

    /// <summary>
    /// Handles notification events from monitored items.
    /// This is a critical path for real-time data.
    /// </summary>
    private void OnNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
    {
        try
        {
            if (e.NotificationValue is not MonitoredItemNotification notification)
            {
                return;
            }

            var value = notification.Value;
            if (value == null)
            {
                return;
            }

            var dataPoint = new DataPoint(
                monitoredItem.StartNodeId.ToString(),
                value.Value,
                value.SourceTimestamp,
                value.StatusCode,
                DateTime.UtcNow.Ticks
            );
            
            // Raise the event to pass the data point to the protocol client
            OnDataReceived?.Invoke(dataPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OPC UA notification for item {DisplayName}", monitoredItem.DisplayName);
        }
    }
}