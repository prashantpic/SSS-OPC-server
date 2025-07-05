using SSS.Services.Notification.Domain.Enums;

namespace SSS.Services.Notification.Infrastructure.Templates.Abstractions;

/// <summary>
/// Defines the contract for a service that can render notification content from a template.
/// This decouples the logic of message creation from the core notification service,
/// allowing template rendering to be a distinct, swappable component.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Renders a template into a subject and body string based on the provided data.
    /// </summary>
    /// <param name="channelType">The channel for which the template should be rendered (e.g., to select HTML vs. plain text).</param>
    /// <param name="templateId">The unique identifier of the template.</param>
    /// <param name="data">An object or dictionary containing the data to populate the template.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a tuple containing the
    /// rendered Subject and Body strings.
    /// </returns>
    Task<(string Subject, string Body)> RenderTemplateAsync(
        NotificationChannel channelType,
        string templateId,
        object data,
        CancellationToken cancellationToken);
}