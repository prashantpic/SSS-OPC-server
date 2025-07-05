using Opc.Client.Core.Application.Interfaces;
using Opc.Ua;
using Opc.Ua.Client;

namespace Opc.Client.Core.Infrastructure.Opc.Protocols;

/// <summary>
/// A concrete client for receiving and acknowledging alarms and conditions from OPC A&C servers.
/// </summary>
/// <remarks>
/// This implementation targets the OPC UA Alarms & Conditions (A&C) profile.
/// </remarks>
public class OpcAcClient
{
    private readonly Session _session;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcAcClient"/> class.
    /// </summary>
    /// <param name="session">An active, connected OPC UA session.</param>
    public OpcAcClient(Session session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    /// <summary>
    /// Subscribes to alarm and condition events from the server.
    /// </summary>
    /// <param name="onAlarm">The callback action to execute when an alarm event is received.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SubscribeToAlarmsAsync(Action<AlarmEventNotification> onAlarm, CancellationToken cancellationToken)
    {
        var subscription = new Subscription(_session.DefaultSubscription) { PublishingInterval = 1000 };

        var monitoredItem = new MonitoredItem(subscription.DefaultItem)
        {
            StartNodeId = Opc.Ua.ObjectIds.Server,
            AttributeId = Attributes.EventNotifier,
            MonitoringMode = MonitoringMode.Reporting,
            QueueSize = 1000
        };

        // Create an event filter to select specific fields from the alarm event.
        var filter = new EventFilter();
        filter.SelectClauses.Add(new SimpleAttributeOperand(Opc.Ua.ObjectTypeIds.BaseEventType, "EventId"));
        filter.SelectClauses.Add(new SimpleAttributeOperand(Opc.Ua.ObjectTypeIds.BaseEventType, "Message"));
        filter.SelectClauses.Add(new SimpleAttributeOperand(Opc.Ua.ObjectTypeIds.BaseEventType, "Severity"));
        filter.SelectClauses.Add(new SimpleAttributeOperand(Opc.Ua.ObjectTypeIds.BaseEventType, "Time"));
        filter.SelectClauses.Add(new SimpleAttributeOperand(Opc.Ua.ObjectTypeIds.BaseEventType, "SourceName"));

        monitoredItem.Filter = filter;
        monitoredItem.Notification += (item, args) => OnAlarmNotification(item, args, onAlarm);
        
        subscription.AddItem(monitoredItem);
        _session.AddSubscription(subscription);
        subscription.Create();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Acknowledges a specific alarm condition on the server.
    /// </summary>
    /// <param name="ackInfo">Information required to acknowledge the alarm.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AcknowledgeAlarmAsync(AcknowledgeInfo ackInfo, CancellationToken cancellationToken)
    {
        var methodToCall = new CallMethodRequest
        {
            ObjectId = new NodeId(ackInfo.ConditionId),
            MethodId = new NodeId(Opc.Ua.MethodIds.AcknowledgeableConditionType_Acknowledge),
            InputArguments = new Variant[]
            {
                new(ackInfo.EventId), // EventId as byte[]
                new(new LocalizedText(ackInfo.Comment)) // Comment
            }
        };

        _session.Call(null, new CallMethodRequestCollection { methodToCall }, out var results, out _);

        if (StatusCode.IsBad(results[0].StatusCode))
        {
            throw new ServiceResultException(results[0].StatusCode);
        }

        return Task.CompletedTask;
    }

    private void OnAlarmNotification(MonitoredItem item, MonitoredItemNotificationEventArgs args, Action<AlarmEventNotification> onAlarm)
    {
        if (args.NotificationValue is not EventFieldList eventFields) return;

        try
        {
            var notification = new AlarmEventNotification(
                Guid.Empty, // ServerId should be enriched by the application service
                (string)eventFields.EventFields[4].Value, // SourceName
                (ushort)eventFields.EventFields[2].Value, // Severity
                ((LocalizedText)eventFields.EventFields[1].Value).Text, // Message
                (DateTime)eventFields.EventFields[3].Value); // Time

            onAlarm(notification);
        }
        catch
        {
            // Log parsing error
        }
    }
}

// --- Placeholder types for compilation ---

/// <summary>
/// Contains the information needed to acknowledge an alarm.
/// </summary>
/// <param name="ConditionId">The NodeId of the alarm condition instance.</param>
/// <param name="EventId">The unique identifier for the event to acknowledge.</param>
/// <param name="Comment">A comment from the user acknowledging the alarm.</param>
public record AcknowledgeInfo(string ConditionId, byte[] EventId, string Comment);