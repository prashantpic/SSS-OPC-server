using System;

namespace OPC.Client.Core.Exceptions
{
    /// <summary>
    /// Custom exception for errors occurring during general OPC protocol communication
    /// (e.g., connection failure, read/write errors).
    /// Provides context like server address and operation type.
    /// REQ-CSVC-001, REQ-CSVC-006.
    /// </summary>
    [Serializable]
    public class OpcCommunicationException : Exception
    {
        /// <summary>
        /// Gets the address of the OPC server involved in the failed communication.
        /// </summary>
        public string? ServerAddress { get; }

        /// <summary>
        /// Gets the type of OPC operation that failed (e.g., "Read", "Write", "Connect").
        /// </summary>
        public string? OperationType { get; }

        public OpcCommunicationException() { }

        public OpcCommunicationException(string message)
            : base(message) { }

        public OpcCommunicationException(string message, Exception innerException)
            : base(message, innerException) { }

        public OpcCommunicationException(string message, string? serverAddress, string? operationType)
            : base(message)
        {
            ServerAddress = serverAddress;
            OperationType = operationType;
        }

        public OpcCommunicationException(string message, string? serverAddress, string? operationType, Exception innerException)
            : base(message, innerException)
        {
            ServerAddress = serverAddress;
            OperationType = operationType;
        }

        protected OpcCommunicationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            ServerAddress = info.GetString("ServerAddress");
            OperationType = info.GetString("OperationType");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ServerAddress", ServerAddress);
            info.AddValue("OperationType", OperationType);
        }
    }
}