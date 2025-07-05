namespace SSS.Services.Notification.Domain.Enums;

/// <summary>
/// Defines the supported notification channels for dispatching messages.
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Represents the email (SMTP) channel.
    /// </summary>
    Email,
    
    /// <summary>
    /// Represents the Short Message Service (SMS) channel.
    /// </summary>
    Sms
}