using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace Opc.System.Shared.Utilities.Monitoring
{
    /// <summary>
    /// Configures and registers OpenTelemetry services for tracing and metrics.
    /// This method should be called in the application's startup code to enable observability features.
    /// </summary>
    public static class OpenTelemetryConfigurator
    {
        /// <summary>
        /// Centralizes the configuration of OpenTelemetry for distributed tracing and metrics.
        /// </summary>
        /// <param name="services">The service collection to add OpenTelemetry services to.</param>
        /// <param name="configuration">The application configuration for reading settings.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceName = configuration["Observability:ServiceName"] ?? "UnknownService";
            var otlpExporterEndpoint = configuration["Observability:OtlpExporterEndpoint"];
            var enablePrometheusExporter = configuration.GetValue<bool>("Observability:EnablePrometheusExporter");

            var resourceBuilder = ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: "1.0.0");

            services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddSource(SystemMetrics.ActivitySource.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddConsoleExporter(); // Always add console exporter for local dev convenience

                    if (!string.IsNullOrWhiteSpace(otlpExporterEndpoint))
                    {
                        tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(otlpExporterEndpoint);
                        });
                    }
                })
                .WithMetrics(meterProviderBuilder =>
                {
                    meterProviderBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddMeter(SystemMetrics.Meter.Name)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();

                    if (enablePrometheusExporter)
                    {
                        meterProviderBuilder.AddPrometheusExporter();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(otlpExporterEndpoint))
                    {
                        meterProviderBuilder.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(otlpExporterEndpoint);
                        });
                    }
                });

            return services;
        }
    }
}