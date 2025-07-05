namespace SSS.Services.Notification.Configuration;

/// <summary>
/// A wrapper for all notification related configuration sections.
/// </summary>
public class NotificationSettings
{
    public const string SectionName = "NotificationSettings";
    
    public AdminRecipients AdminRecipients { get; init; } = new();
    public EmailSettings EmailSettings { get; init; } = new();
    public SmsSettings SmsSettings { get; init; } = new();
}

/// <summary>
/// Represents the configuration for SMTP email delivery.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "NotificationSettings:EmailSettings";

    public string? SmtpServer { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? FromAddress { get; init; }
    public string? FromName { get; init; }
}

/// <summary>
/// Represents the configuration for SMS delivery via a third-party provider.
/// </summary>
public class SmsSettings
{
    public const string SectionName = "NotificationSettings:SmsSettings";

    public string? AccountSid { get; init; }
    public string? AuthToken { get; init; }
    public string? FromPhoneNumber { get; init; }
}

/// <summary>
/// Represents the pre-configured list of administrator contacts for system-wide alerts.
/// </summary>
public class AdminRecipients
{
    public const string SectionName = "NotificationSettings:AdminRecipients";
    
    public List<string> Emails { get; init; } = [];
    public List<string> PhoneNumbers { get; init; } = [];
}