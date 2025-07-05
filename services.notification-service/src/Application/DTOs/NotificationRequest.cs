using SSS.Services.Notification.Domain.Enums;
// This 'using' assumes a shared project 'SSS.Shared.Events' exists.
// The types from the SDS are defined here for context if the project is not present.
// namespace SSS.Shared.Events
// {
//     public record RecipientInfo(string? EmailAddress, string? PhoneNumber);
// }
using SSS.Shared.Events; 

namespace SSS.Services.Notification.Application.DTOs;

/// <summary>
/// Data Transfer Object representing a request to send a notification.
/// This object is used to pass all necessary data from the presentation layer (consumers)
/// to the application service for processing.
/// </summary>
/// <param name="Recipients">A list of recipients for the notification.</param>
/// <param name="TargetChannels">The list of channels (e.g., Email, SMS) to attempt delivery on.</param>
/// <param name="TemplateId">The unique identifier for the template to be rendered (e.g., "CriticalAlarm").</param>
/// <param name="TemplateData">A dictionary of key-value pairs to be used for personalizing the template content.</param>
public record NotificationRequest(
    List<RecipientInfo> Recipients,
    List<NotificationChannel> TargetChannels,
    string TemplateId,
    Dictionary<string, object> TemplateData
);