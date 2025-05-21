namespace OPC.Client.Core.Domain.ValueObjects
{
    using System;

    /// <summary>
    /// Value object representing OPC UA security configuration settings.
    /// Encapsulates all necessary OPC UA security parameters (policy URI, message security mode,
    /// certificate identifiers) for establishing a secure connection.
    /// Implements REQ-3-001.
    /// </summary>
    public record UaSecurityConfiguration
    {
        /// <summary>
        /// The OPC UA Security Policy URI (e.g., "http://opcfoundation.org/UA/SecurityPolicy#Basic256Sha256").
        /// If null or empty, the client will attempt to select the best available.
        /// </summary>
        public string? SecurityPolicyUri { get; }

        /// <summary>
        /// The OPC UA Message Security Mode (e.g., "SignAndEncrypt", "Sign", "None").
        /// Corresponds to Opc.Ua.MessageSecurityMode enum.
        /// </summary>
        public string MessageSecurityMode { get; }

        /// <summary>
        /// The thumbprint of the client certificate to use for secure communication.
        /// Used if ClientCertificateStorePath is also provided.
        /// </summary>
        public string? ClientCertificateThumbprint { get; }

        /// <summary>
        /// The store path for the client certificate (e.g., "CurrentUser\\My" or a directory path).
        /// </summary>
        public string? ClientCertificateStorePath { get; }

        /// <summary>
        /// The subject name of the client certificate. Can be used instead of or with Thumbprint.
        /// </summary>
        public string? ClientCertificateSubjectName { get; }


        /// <summary>
        /// Path to the client certificate file (e.g., PFX or DER).
        /// If set, this is used instead of store-based lookup.
        /// </summary>
        public string? ClientCertificateFilePath { get; }

        /// <summary>
        /// Password for the client certificate file (if it's a PFX file).
        /// </summary>
        public string? ClientCertificateFilePassword { get; }

        /// <summary>
        /// The store path for trusted peer (server) certificates.
        /// </summary>
        public string? TrustedPeerCertificateStorePath { get; }

        /// <summary>
        /// The store path for rejected peer certificates.
        /// </summary>
        public string? RejectedCertificateStorePath { get; }

        /// <summary>
        /// The store path for trusted issuer (CA) certificates.
        /// </summary>
        public string? IssuerCertificateStorePath { get; }

        /// <summary>
        /// Specifies whether to automatically accept untrusted server certificates.
        /// WARNING: Setting this to true is a security risk and should only be used in controlled environments.
        /// </summary>
        public bool AutoAcceptUntrustedCertificates { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UaSecurityConfiguration"/> record.
        /// </summary>
        public UaSecurityConfiguration(
            string messageSecurityMode,
            string? securityPolicyUri = null,
            string? clientCertificateThumbprint = null,
            string? clientCertificateStorePath = null,
            string? clientCertificateSubjectName = null,
            string? clientCertificateFilePath = null,
            string? clientCertificateFilePassword = null,
            string? trustedPeerCertificateStorePath = Constants.TrustedPeerCertificateStorePathConfigKey, // Default from constants
            string? rejectedCertificateStorePath = Constants.RejectedCertificateStorePathConfigKey, // Default from constants
            string? issuerCertificateStorePath = Constants.IssuerCertificateStorePathConfigKey,   // Default from constants
            bool autoAcceptUntrustedCertificates = false)
        {
            if (string.IsNullOrWhiteSpace(messageSecurityMode))
                throw new ArgumentException("MessageSecurityMode cannot be null or empty.", nameof(messageSecurityMode));

            // Validate that if security is enabled, certificate details are provided
            if (messageSecurityMode.Equals("Sign", StringComparison.OrdinalIgnoreCase) ||
                messageSecurityMode.Equals("SignAndEncrypt", StringComparison.OrdinalIgnoreCase))
            {
                bool storeConfigured = !string.IsNullOrWhiteSpace(clientCertificateStorePath) &&
                                       (!string.IsNullOrWhiteSpace(clientCertificateThumbprint) ||
                                        !string.IsNullOrWhiteSpace(clientCertificateSubjectName));
                bool fileConfigured = !string.IsNullOrWhiteSpace(clientCertificateFilePath);

                if (!storeConfigured && !fileConfigured)
                {
                    // Log warning or throw, depending on strictness. For now, allow creation but log.
                    // Console.WriteLine("Warning: Security mode is enabled but no client certificate (store or file) is configured.");
                }
            }

            MessageSecurityMode = messageSecurityMode;
            SecurityPolicyUri = securityPolicyUri;
            ClientCertificateThumbprint = clientCertificateThumbprint;
            ClientCertificateStorePath = clientCertificateStorePath;
            ClientCertificateSubjectName = clientCertificateSubjectName;
            ClientCertificateFilePath = clientCertificateFilePath;
            ClientCertificateFilePassword = clientCertificateFilePassword;
            TrustedPeerCertificateStorePath = trustedPeerCertificateStorePath;
            RejectedCertificateStorePath = rejectedCertificateStorePath;
            IssuerCertificateStorePath = issuerCertificateStorePath;
            AutoAcceptUntrustedCertificates = autoAcceptUntrustedCertificates;
        }

        /// <summary>
        /// Creates a configuration for "None" security.
        /// </summary>
        public static UaSecurityConfiguration None() =>
            new UaSecurityConfiguration(messageSecurityMode: "None");
    }
}