using System;

namespace OPC.Client.Core.Exceptions
{
    /// <summary>
    /// Custom exception for errors during communication (gRPC/RabbitMQ)
    /// with the backend server-side application.
    /// Specific exception type for failures occurring during gRPC calls or
    /// message queue communication with the central server-side application.
    /// REQ-SAP-003.
    /// </summary>
    [Serializable]
    public class ServerConnectivityException : Exception
    {
        /// <summary>
        /// Gets the target service or endpoint that the client was trying to connect to.
        /// </summary>
        public string? TargetService { get; }

        /// <summary>
        /// Gets the type of communication that failed (e.g., "gRPC", "RabbitMQ").
        /// </summary>
        public string? CommunicationType { get; }

        public ServerConnectivityException() { }

        public ServerConnectivityException(string message)
            : base(message) { }

        public ServerConnectivityException(string message, Exception innerException)
            : base(message, innerException) { }

        public ServerConnectivityException(string message, string? targetService, string? communicationType)
            : base(message)
        {
            TargetService = targetService;
            CommunicationType = communicationType;
        }

        public ServerConnectivityException(string message, string? targetService, string? communicationType, Exception innerException)
            : base(message, innerException)
        {
            TargetService = targetService;
            CommunicationType = communicationType;
        }

        protected ServerConnectivityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            TargetService = info.GetString("TargetService");
            CommunicationType = info.GetString("CommunicationType");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("TargetService", TargetService);
            info.AddValue("CommunicationType", CommunicationType);
        }
    }
}