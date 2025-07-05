using Microsoft.Extensions.Logging;
using SSS.Services.Notification.Domain.Enums;
using SSS.Services.Notification.Infrastructure.Channels.Abstractions;
using SSS.Services.Notification.Infrastructure.Channels.Sms.Abstractions;
using SSS.Shared.Events;

namespace SSS.Services.Notification.Infrastructure.Channels.Sms;

/// <summary>
/// The concrete strategy implementation of INotificationChannel for sending SMS messages.
/// It uses an ISmsProvider adapter to interact with the actual SMS gateway (e.g., Twilio).
/// </summary>
public class SmsChannel : INotificationChannel
{
    private readonly ISmsProvider _smsProvider;
    private readonly ILogger<SmsChannel> _logger;

    public SmsChannel(ISmsProvider smsProvider, ILogger<SmsChannel> logger)
    {
        _smsProvider = smsProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public NotificationChannel ChannelType => NotificationChannel.Sms;
    
    /// <inheritdoc />
    public async Task SendAsync(RecipientInfo recipient, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipient.PhoneNumber))
        {
            _logger.LogWarning("Skipping SMS notification for recipient because phone number is not provided.");
            return;
        }

        _logger.LogDebug("Dispatching notification via SmsChannel to {PhoneNumber}", recipient.PhoneNumber);
        
        // The 'subject' parameter is intentionally ignored for the SMS channel.
        await _smsProvider.SendSmsAsync(
            recipient.PhoneNumber,
            body,
            cancellationToken);
    }
}