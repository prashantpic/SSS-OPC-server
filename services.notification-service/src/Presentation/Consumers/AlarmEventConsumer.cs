using MassTransit;
using Microsoft.Extensions.Logging;
using SSS.Services.Notification.Application.Abstractions;
using SSS.Services.Notification.Application.DTOs;
using SSS.Services.Notification.Domain.Enums;
using SSS.Shared.Events;

namespace SSS.Services.Notification.Presentation.Consumers;

/// <summary>
/// A message queue consumer that listens for `AlarmTriggeredEvent` messages and
/// initiates the notification process. This acts as an entry point for alarm-based
/// notifications, decoupling the notification logic from the alarm generation source.
/// </summary>
public class AlarmEventConsumer : IConsumer<AlarmTriggeredEvent>
{
    private readonly ILogger<AlarmEventConsumer> _logger;
    private readonly INotificationService _notificationService;

    public AlarmEventConsumer(ILogger<AlarmEventConsumer> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Consumes the `AlarmTriggeredEvent` message from the message bus.
    /// </summary>
    /// <param name="context">The consume context providing the message and metadata.</param>
    public async Task Consume(ConsumeContext<AlarmTriggeredEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received AlarmTriggeredEvent with CorrelationId: {CorrelationId}. Source: {SourceNode}",
            message.CorrelationId, message.SourceNode);

        var templateData = new Dictionary<string, object>
        {
            { "alarm_message", message.AlarmMessage },
            { "severity", message.Severity },
            { "source_node", message.SourceNode },
            { "timestamp", message.Timestamp }
        };

        var notificationRequest = new NotificationRequest(
            Recipients: message.Recipients,
            TargetChannels: [NotificationChannel.Email, NotificationChannel.Sms],
            TemplateId: "CriticalAlarm",
            TemplateData: templateData
        );

        try
        {
            var success = await _notificationService.SendNotificationAsync(notificationRequest, context.CancellationToken);
            if (success)
            {
                _logger.LogInformation("Successfully processed notification request for CorrelationId: {CorrelationId}", message.CorrelationId);
            }
            else
            {
                _logger.LogWarning("Notification request for CorrelationId: {CorrelationId} completed, but one or more dispatches may have failed.", message.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing notification for CorrelationId: {CorrelationId}. The message will be moved to the error queue.", message.CorrelationId);
            throw; // Re-throwing will trigger MassTransit's retry/error handling policy.
        }
    }
}