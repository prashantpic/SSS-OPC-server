using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.CrossCuttingConcerns.ErrorHandling
{
    /// <summary>
    /// Provides a centralized mechanism for catching and handling un-caught exceptions 
    /// that might occur during the service's operation, ensuring graceful error logging 
    /// and potential recovery attempts.
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        // private readonly IDataTransmissionService _dataTransmissionService; // Optional: to send critical errors to server

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger /*, IDataTransmissionService dataTransmissionService = null */)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // _dataTransmissionService = dataTransmissionService;
        }

        public void SetupGlobalExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                HandleException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException");
                // Potentially decide if the process should terminate based on args.IsTerminating
            };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                HandleException(args.Exception, "TaskScheduler.UnobservedTaskException");
                args.SetObserved(); // Marks the exception as handled
            };
        }

        public void HandleException(Exception? exception, string context)
        {
            if (exception == null)
            {
                _logger.LogError("GlobalExceptionHandler received a null exception object from context: {Context}.", context);
                return;
            }

            _logger.LogCritical(exception, "Unhandled exception caught in {Context}. Exception Type: {ExceptionType}, Message: {ExceptionMessage}",
                context, exception.GetType().FullName, exception.Message);

            // TODO: Add more sophisticated error handling logic:
            // 1. Determine if the exception is fatal or recoverable.
            // 2. Report critical errors to the server application via IDataTransmissionService if available and appropriate.
            //    Example:
            //    if (_dataTransmissionService != null)
            //    {
            //        var auditEvent = new AuditEventDto { ... error details ... };
            //        _dataTransmissionService.SendAuditEventAsync(auditEvent).ConfigureAwait(false).GetAwaiter().GetResult(); // Fire and forget with caution
            //    }
            // 3. Implement graceful shutdown procedures for fatal errors.
            // 4. For worker services, consider strategies to restart or indicate a failed state to an orchestrator.
        }
    }
}