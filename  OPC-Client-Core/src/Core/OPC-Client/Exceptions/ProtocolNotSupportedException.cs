using System;

namespace OPC.Client.Core.Exceptions
{
    /// <summary>
    /// Custom exception thrown when an operation is attempted with an OPC protocol
    /// version or type not supported by the client.
    /// Indicates that a requested OPC protocol (e.g., a specific DA version or
    /// a non-configured protocol) is not supported by the current client instance.
    /// REQ-CSVC-001.
    /// </summary>
    [Serializable]
    public class ProtocolNotSupportedException : Exception
    {
        /// <summary>
        /// Gets the name or identifier of the OPC protocol that was requested but is not supported.
        /// </summary>
        public string? RequestedProtocol { get; }

        public ProtocolNotSupportedException() { }

        public ProtocolNotSupportedException(string message)
            : base(message) { }

        public ProtocolNotSupportedException(string message, Exception innerException)
            : base(message, innerException) { }

        public ProtocolNotSupportedException(string message, string? requestedProtocol)
            : base(message)
        {
            RequestedProtocol = requestedProtocol;
        }

        public ProtocolNotSupportedException(string message, string? requestedProtocol, Exception innerException)
            : base(message, innerException)
        {
            RequestedProtocol = requestedProtocol;
        }

        protected ProtocolNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            RequestedProtocol = info.GetString("RequestedProtocol");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("RequestedProtocol", RequestedProtocol);
        }
    }
}