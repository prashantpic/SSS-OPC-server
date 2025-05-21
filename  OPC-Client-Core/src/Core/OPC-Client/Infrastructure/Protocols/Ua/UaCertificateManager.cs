using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using OPC.Client.Core.Utils; // Assuming CertificateLoaderUtil is in this namespace
using OPC.Client.Core.Exceptions;

namespace OPC.Client.Core.Infrastructure.Protocols.Ua
{
    /// <summary>
    /// Provides and manages OPC UA client and server X.509 certificates.
    /// Includes loading from stores and validation helpers.
    /// Used by <see cref="UaSecurityHandler"/>.
    /// </summary>
    /// <remarks>
    /// Handles loading, validation, and provisioning of X.509 certificates.
    /// Implements REQ-3-001.
    /// </remarks>
    public class UaCertificateManager
    {
        private readonly ILogger<UaCertificateManager> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UaCertificateManager"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public UaCertificateManager(ILogger<UaCertificateManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads an X.509 certificate from a specified file path.
        /// </summary>
        /// <param name="filePath">The path to the certificate file (e.g., .pfx, .cer).</param>
        /// <param name="password">The password required to access the certificate private key, if applicable (for .pfx files).</param>
        /// <returns>The loaded X509Certificate2 object.</returns>
        /// <exception cref="OpcSecurityException">Thrown if the certificate cannot be loaded from the file.</exception>
        public X509Certificate2 LoadCertificateFromFile(string filePath, string? password)
        {
            _logger.LogDebug("Attempting to load certificate from file: {FilePath}", filePath);
            try
            {
                return CertificateLoaderUtil.LoadCertificateFromFile(filePath, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load certificate from file: {FilePath}", filePath);
                throw new OpcSecurityException($"Failed to load certificate from file '{filePath}'. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Loads an X.509 certificate from the specified certificate store.
        /// </summary>
        /// <param name="storePath">The path to the certificate store (e.g., "CurrentUser\My", "LocalMachine\My").</param>
        /// <param name="findType">The type of value to find the certificate by (e.g., Thumbprint, SubjectName).</param>
        /// <param name="findValue">The value to search for in the certificate store.</param>
        /// <param name="validOnly">Specifies whether to consider only valid certificates.</param>
        /// <returns>The loaded X509Certificate2 object, or null if not found.</returns>
        /// <exception cref="OpcSecurityException">Thrown if an error occurs while accessing the store or loading the certificate.</exception>
        public X509Certificate2? LoadCertificateFromStore(string storePath, X509FindType findType, object findValue, bool validOnly = true)
        {
            _logger.LogDebug("Attempting to load certificate from store: {StorePath}, FindType: {FindType}, FindValue: '{FindValue}'", storePath, findType, findValue);
            try
            {
                return CertificateLoaderUtil.LoadCertificateFromStore(storePath, findType, findValue, validOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load certificate from store: {StorePath}, FindType: {FindType}, FindValue: '{FindValue}'", storePath, findType, findValue);
                throw new OpcSecurityException($"Failed to load certificate from store '{storePath}' using {findType} for '{findValue}'. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Validates an X.509 certificate based on standard chain validation and optionally custom rules.
        /// This is a basic validation placeholder. OPC UA SDK performs more thorough validation.
        /// </summary>
        /// <param name="certificate">The certificate to validate.</param>
        /// <param name="checkRevocation">Whether to check for revocation (can be slow).</param>
        /// <returns>True if the certificate is considered valid, false otherwise.</returns>
        public bool ValidateCertificate(X509Certificate2 certificate, bool checkRevocation = false)
        {
            if (certificate == null)
            {
                _logger.LogWarning("Attempted to validate a null certificate.");
                return false;
            }

            _logger.LogDebug("Validating certificate: {SubjectName}, Thumbprint: {Thumbprint}", certificate.SubjectName.Name, certificate.Thumbprint);

            try
            {
                // Basic X509Chain validation
                using (X509Chain chain = new X509Chain())
                {
                    chain.ChainPolicy.RevocationMode = checkRevocation ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot; // Common practice
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag; // Default

                    bool isValid = chain.Build(certificate);

                    if (!isValid)
                    {
                        _logger.LogWarning("Certificate validation failed for {SubjectName}. Chain status:", certificate.SubjectName.Name);
                        foreach (X509ChainStatus status in chain.ChainStatus)
                        {
                            _logger.LogWarning("- Status: {Status}, Information: {StatusInformation}", status.Status, status.StatusInformation);
                        }
                        return false;
                    }

                    _logger.LogInformation("Certificate {SubjectName} passed basic chain validation.", certificate.SubjectName.Name);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during certificate validation for {SubjectName}.", certificate.SubjectName.Name);
                return false;
            }
        }
    }
}