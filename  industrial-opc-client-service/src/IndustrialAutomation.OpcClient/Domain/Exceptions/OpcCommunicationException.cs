using System;

namespace IndustrialAutomation.OpcClient.Domain.Exceptions
{
    /// <summary>
    /// Specific exception type for errors occurring during OPC interactions, 
    /// allowing for more granular error handling.
    /// </summary>
    public class OpcCommunicationException : Exception
    {
        /// <summary>
        /// Identifier of the OPC server involved in the failed operation.
        /// </summary>
        public string? ServerId { get; }

        /// <summary>
        /// The specific OPC operation that failed (e.g., "Read", "Write", "Connect").
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// OPC status code associated with the error, if available.
        /// </summary>
        public string? OpcStatusCode { get; }

        public OpcCommunicationException()
        {
        }

        public OpcCommunicationException(string message) : base(message)
        {
        }

        public OpcCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public OpcCommunicationException(string message, string? serverId, string? operation = null, string? opcStatusCode = null, Exception? innerException = null)
            : base(message, innerException)
        {
            ServerId = serverId;
            Operation = operation;
            OpcStatusCode = opcStatusCode;
        }
    }
}