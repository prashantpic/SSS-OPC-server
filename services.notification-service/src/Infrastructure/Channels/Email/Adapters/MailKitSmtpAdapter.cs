using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using SSS.Services.Notification.Configuration;
using SSS.Services.Notification.Infrastructure.Channels.Email.Abstractions;

namespace SSS.Services.Notification.Infrastructure.Channels.Email.Adapters;

/// <summary>
/// An adapter that implements IEmailProvider using the MailKit library to communicate with an SMTP server.
/// It encapsulates the specific implementation details of using MailKit to send emails.
/// </summary>
public class MailKitSmtpAdapter : IEmailProvider
{
    private readonly EmailSettings _settings;
    private readonly ILogger<MailKitSmtpAdapter> _logger;

    public MailKitSmtpAdapter(IOptions<EmailSettings> settings, ILogger<MailKitSmtpAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpServer) || string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            _logger.LogError("SMTP server or From Address is not configured. Cannot send email.");
            throw new InvalidOperationException("Email settings are not properly configured.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName ?? "No-Reply", _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            
            _logger.LogInformation("Connecting to SMTP server {SmtpServer} on port {Port}", _settings.SmtpServer, _settings.Port);
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, 
                _settings.UseSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None, 
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_settings.Username) && !string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogInformation("Authenticating with username {Username}", _settings.Username);
                await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
            }

            _logger.LogInformation("Sending email to {Recipient}", to);
            await client.SendAsync(message, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {Recipient}. Disconnecting from SMTP server.", to);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via MailKit to {Recipient}. Subject: {Subject}", to, subject);
            throw; // Re-throw to allow the calling service to handle the failure.
        }
    }
}