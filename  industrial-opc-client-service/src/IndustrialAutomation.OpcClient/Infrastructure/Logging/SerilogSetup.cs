using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System;

namespace IndustrialAutomation.OpcClient.Infrastructure.Logging
{
    /// <summary>
    /// Centralized setup for the Serilog logging framework, 
    /// reading configuration from appsettings.json to define sinks, formatting, and logging levels.
    /// </summary>
    public static class SerilogSetup
    {
        public static void Configure(HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration)
        {
            var environment = context.HostingEnvironment;

            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration) // Reads from "Serilog" section in appsettings.json
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", environment.ApplicationName)
                .Enrich.WithProperty("EnvironmentName", environment.EnvironmentName)
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails(); // For better exception logging

            if (environment.IsDevelopment())
            {
                loggerConfiguration.MinimumLevel.Debug(); // Override for development
                loggerConfiguration.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                // Example: configure file sink for production, could also be read from config
                // loggerConfiguration.WriteTo.File(
                //    "logs/opc-client-.log",
                //    rollingInterval: RollingInterval.Day,
                //    restrictedToMinimumLevel: LogEventLevel.Information,
                //    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                // );
            }

            // Fallback if no sinks are configured via appsettings.json
            if (loggerConfiguration.GetType().GetProperty("LoggerSinks", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(loggerConfiguration) is Array sinks && sinks.Length == 0)
            {
                 loggerConfiguration.WriteTo.Console(); // Default to console if nothing else is configured
            }
        }
    }
}