using Microsoft.Extensions.Hosting;
using Serilog;

namespace Opc.System.Shared.Utilities.Logging
{
    /// <summary>
    /// Configures and initializes the Serilog logger for an application.
    /// It is designed to be called during the application startup process to ensure consistent logging practices are applied system-wide.
    /// </summary>
    public static class SerilogConfigurator
    {
        /// <summary>
        /// Centralizes the configuration of Serilog for an application.
        /// </summary>
        /// <param name="hostBuilder">The host builder to configure.</param>
        /// <returns>The configured host builder for chaining.</returns>
        /// <remarks>
        /// This method configures Serilog by:
        /// 1. Reading sinks, log levels, and other settings from the application's IConfiguration (e.g., appsettings.json).
        /// 2. Enriching log events with contextual information like MachineName, ProcessId, and ThreadId.
        /// 3. Enabling enrichment from the LogContext.
        /// </remarks>
        public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
        {
            hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
            {
                // Load the bulk of the configuration from appsettings.json
                // This allows for environment-specific overrides for sinks, log levels, etc.
                loggerConfiguration.ReadFrom.Configuration(context.Configuration);

                // Add standard enrichers for valuable contextual data
                loggerConfiguration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProcessId()
                    .Enrich.WithThreadId();
            });

            return hostBuilder;
        }
    }
}