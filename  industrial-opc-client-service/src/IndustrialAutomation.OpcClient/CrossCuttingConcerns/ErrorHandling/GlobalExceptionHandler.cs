using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IndustrialAutomation.OpcClient.CrossCuttingConcerns.ErrorHandling;

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

    public void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        _logger.LogCritical(exception, "Unhandled exception caught. IsTerminating: {IsTerminating}", e.IsTerminating);

        // Optionally, try to send this critical error to the server application
        // if (_dataTransmissionService != null && exception != null)
        // {
        //     var auditEvent = new AuditEventDto
        //     {
        //         ClientId = "Unknown", // Attempt to get ClientId if possible
        //         Timestamp = DateTime.UtcNow,
        //         EventType = "UnhandledException",
        //         Source = "GlobalExceptionHandler",
        //         Description = $"Unhandled exception: {exception.Message}",
        //         Details = new Dictionary<string, string> { { "StackTrace", exception.StackTrace ?? "N/A" } }
        //     };
        //     _dataTransmissionService.SendAuditEventAsync(auditEvent).GetAwaiter().GetResult(); // Fire-and-forget or handle carefully
        // }

        // If the exception is terminating, there's not much more to do than log.
        // If not terminating, consider if any specific recovery actions are feasible,
        // but typically, an unhandled exception in a background service might still lead to instability.
    }

    public Task HandleTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogCritical(e.Exception, "Unobserved task exception caught.");
        e.SetObserved(); // Mark as observed to prevent process termination if possible

        // Similar optional reporting as above
        return Task.CompletedTask;
    }

    public void Register()
    {
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        TaskScheduler.UnobservedTaskException += HandleTaskSchedulerUnobservedTaskException;
        _logger.LogInformation("Global exception handlers registered.");
    }

    public void Unregister()
    {
        AppDomain.CurrentDomain.UnhandledException -= HandleUnhandledException;
        TaskScheduler.UnobservedTaskException -= HandleTaskSchedulerUnobservedTaskException;
        _logger.LogInformation("Global exception handlers unregistered.");
    }
}