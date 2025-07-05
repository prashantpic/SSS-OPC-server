namespace Opc.System.Shared.Utilities.Logging
{
    /// <summary>
    /// A static container for standardized property names for structured logging.
    /// Using these constants ensures that all services use the same keys for the same type of information,
    /// simplifying log aggregation and analysis.
    /// </summary>
    public static class LogEventProperties
    {
        /// <summary>
        /// The name of the application or service generating the log.
        /// </summary>
        public const string ApplicationName = "ApplicationName";

        /// <summary>
        /// A unique identifier for a single request or transaction, used to trace it across multiple services.
        /// </summary>
        public const string CorrelationId = "CorrelationId";

        /// <summary>
        /// The identifier of the user who initiated the action being logged.
        /// </summary>
        public const string UserId = "UserId";

        /// <summary>
        /// The endpoint URL of the OPC Server involved in the logged operation.
        /// </summary>
        public const string OpcServerEndpoint = "OpcServerEndpoint";

        /// <summary>
        /// The identifier for the tenant in a multi-tenant environment.
        /// </summary>
        public const string TenantId = "TenantId";
    }
}