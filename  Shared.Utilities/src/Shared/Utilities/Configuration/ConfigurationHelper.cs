using Microsoft.Extensions.Configuration;
using System;

namespace Opc.System.Shared.Utilities.Configuration
{
    /// <summary>
    /// Provides a static method to create a standard IConfiguration object for an application.
    /// It aggregates settings from JSON files, environment variables, and command-line arguments.
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Ensures all applications build their configuration stack consistently.
        /// </summary>
        /// <param name="args">Command line arguments, which are added as a configuration source.</param>
        /// <returns>An <see cref="IConfiguration"/> instance built from standard sources.</returns>
        /// <remarks>
        /// The method adds sources in the following order of precedence (later sources override earlier ones):
        /// 1. appsettings.json (required)
        /// 2. appsettings.{Environment}.json (optional)
        /// 3. Environment Variables
        /// 4. Command Line Arguments
        /// </remarks>
        public static IConfiguration BuildConfiguration(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            return builder.Build();
        }
    }
}