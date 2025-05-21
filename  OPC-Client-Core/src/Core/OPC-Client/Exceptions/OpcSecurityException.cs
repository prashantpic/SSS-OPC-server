using System;

namespace OPC.Client.Core.Exceptions
{
    /// <summary>
    /// Custom exception for errors related to OPC UA security mechanisms
    /// (e.g., certificate validation failure, authentication denied).
    /// Specific exception type for failures related to OPC UA security mechanisms,
    /// such as issues with certificates, user authentication, or secure channel establishment.
    /// REQ-3-001.
    /// </summary>
    [Serializable]
    public class OpcSecurityException : Exception
    {
        /// <summary>
        /// Gets the security policy URI that was in effect or attempted.
        /// </summary>
        public string? SecurityPolicyUri { get; }

        /// <summary>
        /// Gets the type of security failure (e.g., "CertificateValidation", "Authentication").
        /// </summary>
        public string? FailureType { get; }


        public OpcSecurityException() { }

        public OpcSecurityException(string message)
            : base(message) { }

        public OpcSecurityException(string message, Exception innerException)
            : base(message, innerException) { }

        public OpcSecurityException(string message, string? failureType, string? securityPolicyUri = null)
            : base(message)
        {
            FailureType = failureType;
            SecurityPolicyUri = securityPolicyUri;
        }

        public OpcSecurityException(string message, string? failureType, string? securityPolicyUri, Exception innerException)
            : base(message, innerException)
        {
            FailureType = failureType;
            SecurityPolicyUri = securityPolicyUri;
        }

        protected OpcSecurityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            SecurityPolicyUri = info.GetString("SecurityPolicyUri");
            FailureType = info.GetString("FailureType");
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SecurityPolicyUri", SecurityPolicyUri);
            info.AddValue("FailureType", FailureType);
        }
    }
}