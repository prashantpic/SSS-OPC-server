using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Opc.System.Shared.Utilities.Monitoring
{
    /// <summary>
    /// Provides global, statically accessible OpenTelemetry ActivitySource and Meter instances,
    /// along with predefined instruments for common system KPIs.
    /// This ensures all parts of the application use the same source for traces and metrics.
    /// </summary>
    public static class SystemMetrics
    {
        /// <summary>
        /// The single <see cref="ActivitySource"/> for creating activities (spans) for distributed tracing.
        /// Use this to start and stop activities for operations you want to trace.
        /// </summary>
        public static readonly ActivitySource ActivitySource = new ActivitySource(MonitoringConstants.ActivitySourceName);

        /// <summary>
        /// The single <see cref="Meter"/> for creating metric instruments.
        /// Use this to create counters, histograms, and other metric types.
        /// </summary>
        public static readonly Meter Meter = new Meter(MonitoringConstants.MeterName);

        /// <summary>
        /// A counter that measures the total number of API requests received.
        /// </summary>
        public static readonly Counter<long> ApiRequestCounter = Meter.CreateCounter<long>(
            name: "api_requests_total",
            unit: "requests",
            description: "Total number of API requests.");

        /// <summary>
        /// A histogram that measures the duration of API requests.
        /// </summary>
        public static readonly Histogram<double> ApiRequestDuration = Meter.CreateHistogram<double>(
            name: "api_requests_duration_ms",
            unit: "ms",
            description: "Duration of API requests in milliseconds.");

        /// <summary>
        /// A counter that measures the total number of OPC write operations performed.
        /// </summary>
        public static readonly Counter<long> OpcWriteOperationsCounter = Meter.CreateCounter<long>(
            name: "opc_write_ops_total",
            unit: "operations",
            description: "Total number of OPC write operations performed.");
    }
}