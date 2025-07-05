using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSS.Services.Notification.Configuration;
using SSS.Services.Notification.Infrastructure.Channels.Sms.Abstractions;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SSS.Services.Notification.Infrastructure.Channels.Sms.Adapters;

/// <summary>
/// An adapter that implements ISmsProvider using the Twilio SDK to send SMS messages.
/// It encapsulates the specific implementation details of using the Twilio API.
/// </summary>
public class TwilioSmsAdapter : ISmsProvider
{
    private readonly SmsSettings _settings;
    private readonly ILogger<TwilioSmsAdapter> _logger;

    public TwilioSmsAdapter(IOptions<SmsSettings> settings, ILogger<TwilioSmsAdapter> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.AccountSid) || string.IsNullOrWhiteSpace(_settings.AuthToken))
        {
            _logger.LogWarning("Twilio AccountSid or AuthToken is not configured. The Twilio client will not be initialized.");
        }
        else
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);
        }
    }
    
    /// <inheritdoc />
    public async Task SendSmsAsync(string to, string body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.AccountSid) || string.IsNullOrWhiteSpace(_settings.FromPhoneNumber))
        {
            _logger.LogError("Twilio settings (AccountSid, FromPhoneNumber) are not configured. Cannot send SMS.");
            throw new InvalidOperationException("Twilio settings are not properly configured.");
        }
        
        try
        {
            _logger.LogInformation("Attempting to send SMS to {Recipient} from {FromNumber}", to, _settings.FromPhoneNumber);

            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(to),
                from: new PhoneNumber(_settings.FromPhoneNumber),
                body: body
            );

            if (message.ErrorCode.HasValue)
            {
                _logger.LogError("Twilio reported an error sending SMS to {Recipient}. Code: {ErrorCode}, Message: {ErrorMessage}",
                    to, message.ErrorCode, message.ErrorMessage);
                // Throw an exception to signal failure
                throw new ApiException($"Twilio error {message.ErrorCode}: {message.ErrorMessage}");
            }
            
            _logger.LogInformation("Successfully sent SMS to {Recipient}. SID: {MessageSid}", to, message.Sid);
        }
        catch (ApiException ex)
        {
            _logger.LogError(ex, "Twilio API exception while sending SMS to {Recipient}. Status: {StatusCode}, Code: {ErrorCode}",
                to, ex.StatusCode, ex.Code);
            throw; // Re-throw to allow the calling service to handle the failure.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while sending SMS via Twilio to {Recipient}", to);
            throw;
        }
    }
}