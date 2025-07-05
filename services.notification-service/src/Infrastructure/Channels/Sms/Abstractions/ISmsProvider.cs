namespace SSS.Services.Notification.Infrastructure.Channels.Sms.Abstractions;

/// <summary>
/// Defines the contract for an SMS provider adapter.
/// This decouples the SmsChannel from a specific gateway like Twilio.
/// </summary>
public interface ISmsProvider
{
    /// <summary>
    /// Sends an SMS message.
    /// </summary>
    /// <param name="to">The recipient's phone number in E.164 format.</param>
    /// <param name="body">The text content of the SMS.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendSmsAsync(string to, string body, CancellationToken cancellationToken);
}