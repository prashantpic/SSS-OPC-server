using SSS.Services.Notification.Domain.Enums;
using SSS.Shared.Events;

namespace SSS.Services.Notification.Infrastructure.Channels.Abstractions;

/// <summary>
/// Defines the contract for a notification channel (Strategy Pattern).
/// This allows the application to treat all channels (e.g., Email, SMS) uniformly.
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Gets the type of channel this implementation represents.
    /// </summary>
    NotificationChannel ChannelType { get; }

    /// <summary>
    /// Sends a notification to a single recipient using the specific channel's mechanism.
    /// </summary>
    /// <param name="recipient">The recipient information, containing details like email or phone number.</param>
    /// <param name="subject">The subject of the message (may be ignored by some channels like SMS).</param>
    /// <param name="body">The main content of the message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendAsync(RecipientInfo recipient, string subject, string body, CancellationToken cancellationToken);
}