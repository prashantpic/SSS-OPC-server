namespace OPC.Client.Core.Domain.ValueObjects
{
    using System;

    /// <summary>
    /// Value object representing an OPC UA user identity for authentication.
    /// Holds user credentials or tokens in a structured and immutable way for OPC UA server authentication.
    /// Supports Anonymous, UserName/Password, Certificate, and IssuedToken identity types.
    /// Implements REQ-3-001.
    /// </summary>
    public record UaUserIdentity
    {
        /// <summary>
        /// The type of user identity.
        /// </summary>
        public UserIdentityType Type { get; }

        // Properties for UserName identity
        public string? Username { get; }
        public string? Password { get; } // Note: Storing passwords directly is a security risk. Consider alternatives.

        // Properties for Certificate identity
        public string? CertificateThumbprint { get; } // Thumbprint of the user certificate
        public string? CertificateStorePath { get; }  // Store path for the user certificate
        public string? CertificateFilePath { get; } // File path for the user certificate
        public string? CertificateFilePassword { get; } // Password for the certificate file

        // Properties for IssuedToken identity
        public string? IssuedToken { get; } // The actual token string
        public string? PolicyId { get; }    // PolicyId associated with the IssuedToken (optional)

        /// <summary>
        /// Private constructor to enforce creation via static factory methods.
        /// </summary>
        private UaUserIdentity(
            UserIdentityType type,
            string? username = null,
            string? password = null,
            string? certificateThumbprint = null,
            string? certificateStorePath = null,
            string? certificateFilePath = null,
            string? certificateFilePassword = null,
            string? issuedToken = null,
            string? policyId = null)
        {
            Type = type;
            Username = username;
            Password = password; // Storing password; be cautious.
            CertificateThumbprint = certificateThumbprint;
            CertificateStorePath = certificateStorePath;
            CertificateFilePath = certificateFilePath;
            CertificateFilePassword = certificateFilePassword;
            IssuedToken = issuedToken;
            PolicyId = policyId;
        }

        /// <summary>
        /// Creates an Anonymous user identity.
        /// </summary>
        public static UaUserIdentity Anonymous() =>
            new UaUserIdentity(UserIdentityType.Anonymous);

        /// <summary>
        /// Creates a UserName/Password user identity.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="ArgumentNullException">Thrown if username or password is null.</exception>
        public static UaUserIdentity UserName(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (password == null) throw new ArgumentNullException(nameof(password)); // Password can be empty but not null
            return new UaUserIdentity(UserIdentityType.UserName, username: username, password: password);
        }

        /// <summary>
        /// Creates a Certificate user identity using a certificate from a store.
        /// </summary>
        /// <param name="storePath">The certificate store path.</param>
        /// <param name="thumbprint">The thumbprint of the certificate.</param>
        /// <exception cref="ArgumentNullException">Thrown if storePath or thumbprint is null or empty.</exception>
        public static UaUserIdentity CertificateFromStore(string storePath, string thumbprint)
        {
            if (string.IsNullOrEmpty(storePath)) throw new ArgumentNullException(nameof(storePath));
            if (string.IsNullOrEmpty(thumbprint)) throw new ArgumentNullException(nameof(thumbprint));
            return new UaUserIdentity(UserIdentityType.Certificate, certificateStorePath: storePath, certificateThumbprint: thumbprint);
        }

        /// <summary>
        /// Creates a Certificate user identity using a certificate from a file.
        /// </summary>
        /// <param name="filePath">The path to the certificate file.</param>
        /// <param name="password">The password for the certificate file (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown if filePath is null or empty.</exception>
        public static UaUserIdentity CertificateFromFile(string filePath, string? password = null)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));
            return new UaUserIdentity(UserIdentityType.Certificate, certificateFilePath: filePath, certificateFilePassword: password);
        }

        /// <summary>
        /// Creates an IssuedToken user identity.
        /// </summary>
        /// <param name="token">The issued token string.</param>
        /// <param name="policyId">The policy ID associated with the token (optional).</param>
        /// <exception cref="ArgumentNullException">Thrown if token is null or empty.</exception>
        public static UaUserIdentity IssuedTokenIdentity(string token, string? policyId = null)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
            return new UaUserIdentity(UserIdentityType.IssuedToken, issuedToken: token, policyId: policyId);
        }
    }

    /// <summary>
    /// Defines the types of user identities for OPC UA authentication.
    /// </summary>
    public enum UserIdentityType
    {
        Anonymous,
        UserName,
        Certificate,
        IssuedToken
    }
}