using Microsoft.Extensions.Logging;
using SSS.Services.Notification.Domain.Enums;
using SSS.Services.Notification.Infrastructure.Channels.Abstractions;
using SSS.Services.Notification.Infrastructure.Channels.Email.Abstractions;
using SSS.Shared.Events;

namespace SSS.Services.Notification.Infrastructure.Channels.Email;

/// <summary>
/// The concrete strategy implementation of INotificationChannel for sending emails.
/// It uses an IEmailProvider adapter to interact with the actual email sending service.
/// </summary>
public class EmailChannel : INotificationChannel
{
    private readonly IEmailProvider _emailProvider;
    private readonly ILogger<EmailChannel> _logger;

    public EmailChannel(IEmailProvider emailProvider, ILogger<EmailChannel> logger)
    {
        _emailProvider = emailProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    public NotificationChannel ChannelType => NotificationChannel.Email;

    /// <inheritdoc />
    public async Task SendAsync(RecipientInfo recipient, string subject, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(recipient.EmailAddress))
        {
            _logger.LogWarning("Skipping email notification for recipient because email address is not provided.");
            return;
        }

        _logger.LogDebug("Dispatching notification via EmailChannel to {EmailAddress}", recipient.EmailAddress);
        
        await _emailProvider.SendEmailAsync(
            recipient.EmailAddress,
            subject,
            body,
            cancellationToken);
    }
}