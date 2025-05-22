using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System;

namespace IndustrialAutomation.OpcClient.Infrastructure.Logging;

/// <summary>
/// Centralized setup for the Serilog logging framework, 
/// reading configuration from appsettings.json to define sinks, formatting, and logging levels.
/// </summary>
public static class SerilogSetup
{
    public static void Configure(HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration)
    {
        var environment = context.HostingEnvironment;
        var configuration = context.Configuration;

        loggerConfiguration
            .ReadFrom.Configuration(configuration) // Reads "Serilog" section from appsettings.json
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", environment.ApplicationName)
            .Enrich.WithProperty("EnvironmentName", environment.EnvironmentName)
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails(); // For detailed exception logging

        if (environment.IsDevelopment())
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Debug);
        }
        else
        {
            // Example: Console sink for production (could be file, ELK, etc.)
            loggerConfiguration.WriteTo.Console(
                 outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}", // Or JSON formatter
                 restrictedToMinimumLevel: LogEventLevel.Information);

            // Example: File sink (ensure directory exists and has write permissions)
            // loggerConfiguration.WriteTo.File(
            //    Path.Combine(AppContext.BaseDirectory, "logs", "opc-client-.log"),
            //    rollingInterval: RollingInterval.Day,
            //    restrictedToMinimumLevel: LogEventLevel.Information,
            //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            // );
        }

        // You can add more specific sinks or configurations here,
        // for example, a dedicated sink for critical write logs.
    }
}