using MudBlazor;

namespace Opc.System.UI.Client.Shared.Services.Notification
{
    /// <summary>
    /// Defines a contract for a service that displays transient notifications (snackbars) to the user.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Shows a notification indicating a successful operation.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void ShowSuccess(string message);

        /// <summary>
        /// Shows a notification indicating an error.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void ShowError(string message);

        /// <summary>
        /// Shows a notification with informational content.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void ShowInfo(string message);
    }

    /// <summary>
    /// A client-side service for displaying transient notifications (toasts/snackbars) to the user. 
    /// It acts as a wrapper around the MudBlazor ISnackbar service to provide a centralized
    /// and simplified API for user feedback, as required by REQ-UIX-001.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ISnackbar _snackbar;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="snackbar">The injected MudBlazor snackbar service.</param>
        public NotificationService(ISnackbar snackbar)
        {
            _snackbar = snackbar;
        }

        /// <inheritdoc/>
        public void ShowSuccess(string message)
        {
            _snackbar.Add(message, Severity.Success);
        }

        /// <inheritdoc/>
        public void ShowError(string message)
        {
            _snackbar.Add(message, Severity.Error);
        }

        /// <inheritdoc/>
        public void ShowInfo(string message)
        {
            _snackbar.Add(message, Severity.Info);
        }
    }
}