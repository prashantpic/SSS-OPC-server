using SSS.Services.Notification.Application.DTOs;

namespace SSS.Services.Notification.Application.Abstractions;

/// <summary>
/// Defines the primary contract for the notification orchestration logic.
/// This interface abstracts the process of sending a notification from the entry points (e.g., message consumers).
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Orchestrates the entire process of sending a notification based on the provided request.
    /// This includes template rendering and dispatching through one or more channels.
    /// </summary>
    /// <param name="request">The notification request DTO containing all necessary details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is true if at least one
    /// notification was successfully dispatched; otherwise, false.
    /// </returns>
    Task<bool> SendNotificationAsync(NotificationRequest request, CancellationToken cancellationToken);
}