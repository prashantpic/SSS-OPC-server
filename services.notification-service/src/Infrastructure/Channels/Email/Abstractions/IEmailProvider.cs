namespace SSS.Services.Notification.Infrastructure.Channels.Email.Abstractions;

/// <summary>
/// Defines the contract for an email provider adapter.
/// This decouples the EmailChannel from a specific service implementation like MailKit or SendGrid.
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Sends an email message.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="htmlBody">The body of the email, expected to be in HTML format.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken);
}