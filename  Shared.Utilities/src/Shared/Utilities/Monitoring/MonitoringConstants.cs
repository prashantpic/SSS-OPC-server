namespace Opc.System.Shared.Utilities.Monitoring
{
    /// <summary>
    /// A static container for standardized names and keys used in OpenTelemetry tracing and metrics.
    /// This ensures that all telemetry data is consistently named and tagged,
    /// making it easier to correlate, query, and visualize.
    /// </summary>
    public static class MonitoringConstants
    {
        /// <summary>
        /// The name for the system-wide ActivitySource.
        /// </summary>
        public const string ActivitySourceName = "Opc.System";

        /// <summary>
        /// The name for the system-wide Meter.
        /// </summary>
        public const string MeterName = "Opc.System.Metrics";

        /// <summary>
        /// Tag key for the API endpoint path.
        /// </summary>
        public const string ApiEndpointTag = "api.endpoint";

        /// <summary>
        /// Tag key for the type of OPC operation being performed (e.g., Read, Write, Subscribe).
        /// </summary>
        public const string OpcOperationTag = "opc.operation";

        /// <summary>
        /// Tag key for the status code or result of an OPC operation.
        /// </summary>
        public const string OpcStatusTag = "opc.status_code";
    }
}