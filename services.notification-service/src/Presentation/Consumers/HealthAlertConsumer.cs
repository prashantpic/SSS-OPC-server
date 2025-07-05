using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSS.Services.Notification.Application.Abstractions;
using SSS.Services.Notification.Application.DTOs;
using SSS.Services.Notification.Configuration;
using SSS.Services.Notification.Domain.Enums;
using SSS.Shared.Events;

namespace SSS.Services.Notification.Presentation.Consumers;

/// <summary>
/// A message queue consumer that listens for `HealthAlertEvent` messages
/// and notifies a pre-configured list of administrators.
/// </summary>
public class HealthAlertConsumer : IConsumer<HealthAlertEvent>
{
    private readonly ILogger<HealthAlertConsumer> _logger;
    private readonly INotificationService _notificationService;
    private readonly NotificationSettings _settings;

    public HealthAlertConsumer(
        ILogger<HealthAlertConsumer> logger,
        INotificationService notificationService,
        IOptions<NotificationSettings> settings)
    {
        _logger = logger;
        _notificationService = notificationService;
        _settings = settings.Value;
    }
    
    /// <summary>
    /// Consumes the `HealthAlertEvent` message from the message bus.
    /// </summary>
    /// <param name="context">The consume context providing the message and metadata.</param>
    public async Task Consume(ConsumeContext<HealthAlertEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received HealthAlertEvent with CorrelationId: {CorrelationId} for Component: {Component}",
            message.CorrelationId, message.Component);

        var adminRecipients = new List<RecipientInfo>();
        adminRecipients.AddRange(_settings.AdminRecipients.Emails.Select(email => new RecipientInfo(email, null)));
        adminRecipients.AddRange(_settings.AdminRecipients.PhoneNumbers.Select(phone => new RecipientInfo(null, phone)));

        if (adminRecipients.Count == 0)
        {
            _logger.LogWarning("Received HealthAlertEvent but no administrator recipients are configured. The alert will be dropped.");
            return;
        }

        var templateData = new Dictionary<string, object>
        {
            { "alert_message", message.AlertMessage },
            { "component", message.Component },
            { "timestamp", message.Timestamp }
        };

        var notificationRequest = new NotificationRequest(
            Recipients: adminRecipients,
            TargetChannels: [NotificationChannel.Email, NotificationChannel.Sms],
            TemplateId: "HealthAlert",
            TemplateData: templateData
        );

        try
        {
            var success = await _notificationService.SendNotificationAsync(notificationRequest, context.CancellationToken);
            if (success)
            {
                _logger.LogInformation("Successfully processed health alert notification for CorrelationId: {CorrelationId}", message.CorrelationId);
            }
            else
            {
                 _logger.LogWarning("Health alert notification for CorrelationId: {CorrelationId} completed, but one or more dispatches may have failed.", message.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing health alert for CorrelationId: {CorrelationId}. The message will be moved to the error queue.", message.CorrelationId);
            throw; // Re-throwing will trigger MassTransit's retry/error handling policy.
        }
    }
}