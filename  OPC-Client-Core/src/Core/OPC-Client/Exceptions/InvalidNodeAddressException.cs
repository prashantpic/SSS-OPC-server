using System;

namespace OPC.Client.Core.Exceptions
{
    /// <summary>
    /// Custom exception for invalid OPC tag addresses or node identifiers.
    /// Indicates that a provided OPC node address/identifier is malformed,
    /// does not exist on the server, or is otherwise invalid.
    /// REQ-CSVC-006.
    /// </summary>
    [Serializable]
    public class InvalidNodeAddressException : Exception
    {
        /// <summary>
        /// Gets the string representation of the offending node address.
        /// </summary>
        public string? NodeAddressString { get; }

        public InvalidNodeAddressException() { }

        public InvalidNodeAddressException(string message)
            : base(message) { }

        public InvalidNodeAddressException(string message, Exception innerException)
            : base(message, innerException) { }

        public InvalidNodeAddressException(string message, string? nodeAddressString)
            : base(message)
        {
            NodeAddressString = nodeAddressString;
        }

        public InvalidNodeAddressException(string message, string? nodeAddressString, Exception innerException)
            : base(message, innerException)
        {
            NodeAddressString = nodeAddressString;
        }

        protected InvalidNodeAddressException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            NodeAddressString = info.GetString("NodeAddressString");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("NodeAddressString", NodeAddressString);
        }
    }
}