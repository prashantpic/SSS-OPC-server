using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OPC.Client.Core.Exceptions; // Assuming custom exceptions like OpcSecurityException exist

namespace OPC.Client.Core.Utils
{
    /// <summary>
    /// Utility class for loading X.509 certificates from files or certificate stores.
    /// This utility supports OPC UA security requirements by providing reliable certificate access.
    /// Implements features for REQ-3-001.
    /// </summary>
    public static class CertificateLoaderUtil
    {
        // Static logger, or inject ILoggerFactory if used in a non-static context
        private static ILogger? _logger; // Nullable, to be set by an optional Initialize method or used carefully

        /// <summary>
        /// Initializes the logger for CertificateLoaderUtil. Call this at application startup if static logging is desired.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(typeof(CertificateLoaderUtil).FullName ?? "CertificateLoaderUtil");
        }

        /// <summary>
        /// Loads an X.509 certificate from a specified file path.
        /// Supports various certificate file formats like .pfx, .cer, .der.
        /// </summary>
        /// <param name="filePath">The absolute path to the certificate file.</param>
        /// <param name="password">The password required to access the private key in a .pfx file. Optional for other formats.</param>
        /// <returns>The loaded X509Certificate2 object.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the certificate file is not found at the specified path.</exception>
        /// <exception cref="OpcSecurityException">Thrown if an error occurs during certificate loading (e.g., invalid format, incorrect password).</exception>
        public static X509Certificate2 LoadCertificateFromFile(string filePath, string? password = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("Certificate file path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                _logger?.LogError("Certificate file not found at path: {FilePath}", filePath);
                throw new FileNotFoundException($"Certificate file not found: {filePath}", filePath);
            }

            _logger?.LogDebug("Attempting to load certificate from file: {FilePath}", filePath);

            try
            {
                // X509Certificate2 constructor can infer the format for common types (CER, PFX)
                // For PFX files, password is required if the file is password-protected.
                // For public certificates (CER, DER), password is not used.
                X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable;

                if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                {
                    // On Linux/macOS, EphemeralKeySet is often preferred or required, especially in containers,
                    // as MachineKeySet might require elevated privileges or specific setup.
                    // PersistKeySet is generally not applicable without specific configurations.
                    // Exportable is good for debugging or if the key needs to be re-exported, but consider security implications.
                    keyStorageFlags = X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable;
                }


                return new X509Certificate2(filePath, password, keyStorageFlags);
            }
            catch (CryptographicException cryptoEx)
            {
                _logger?.LogError(cryptoEx, "Cryptographic error loading certificate from file {FilePath}. Check file format, password, and permissions.", filePath);
                throw new OpcSecurityException($"Error loading certificate from file '{filePath}'. Ensure the file format is correct, the password (if applicable) is valid, and the application has permissions to access the file and its private key.", cryptoEx);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error loading certificate from file {FilePath}.", filePath);
                throw new OpcSecurityException($"An unexpected error occurred while loading certificate from file '{filePath}'.", ex);
            }
        }

        /// <summary>
        /// Loads an X.509 certificate from the specified Windows certificate store.
        /// </summary>
        /// <param name="storeName">The name of the certificate store (e.g., My, Root, AddressBook).</param>
        /// <param name="storeLocation">The location of the certificate store (e.g., CurrentUser, LocalMachine).</param>
        /// <param name="findType">The type of value to find the certificate by (e.g., FindByThumbprint, FindBySubjectName).</param>
        /// <param name="findValue">The value to search for based on the findType.</param>
        /// <param name="validOnly">Specifies whether to return only certificates that are valid (not expired, not revoked, etc.). Default is false.</param>
        /// <returns>The found X509Certificate2 object, or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown if findValue is null or empty.</exception>
        /// <exception cref="OpcSecurityException">Thrown if an error occurs while accessing the store or certificates.</exception>
        public static X509Certificate2? LoadCertificateFromStore(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue, bool validOnly = false)
        {
            if (findValue == null || (findValue is string s && string.IsNullOrWhiteSpace(s)))
            {
                throw new ArgumentException("Find value for certificate cannot be null or empty.", nameof(findValue));
            }

            _logger?.LogDebug("Attempting to load certificate from store: {StoreLocation}/{StoreName}, FindType: {FindType}, FindValue: '{FindValue}', ValidOnly: {ValidOnly}",
                storeLocation, storeName, findType, findValue, validOnly);

            X509Store? store = null;
            try
            {
                store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection foundCertificates = store.Certificates.Find(findType, findValue, validOnly);

                if (foundCertificates.Count == 0)
                {
                    _logger?.LogWarning("Certificate not found in store {StoreLocation}/{StoreName} for FindType: {FindType}, FindValue: '{FindValue}'",
                        storeLocation, storeName, findType, findValue);
                    return null;
                }

                if (foundCertificates.Count > 1)
                {
                    _logger?.LogWarning("Multiple certificates ({Count}) found in store {StoreLocation}/{StoreName} for FindType: {FindType}, FindValue: '{FindValue}'. Returning the first one.",
                        foundCertificates.Count, storeLocation, storeName, findType, findValue);
                    // Optionally, implement logic to select the "best" certificate (e.g., newest, longest validity)
                }

                // Return a copy to avoid issues if the store is closed, though X509Certificate2 instances are generally usable.
                // Creating a new instance from the raw data ensures it's detached.
                return new X509Certificate2(foundCertificates[0].Export(X509ContentType.Cert), (string?)null, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            }
            catch (CryptographicException cryptoEx)
            {
                _logger?.LogError(cryptoEx, "Cryptographic error accessing certificate store {StoreLocation}/{StoreName} or loading certificate.", storeLocation, storeName);
                throw new OpcSecurityException($"Cryptographic error accessing certificate store '{storeLocation}/{storeName}' or loading certificate.", cryptoEx);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error loading certificate from store {StoreLocation}/{StoreName}.", storeLocation, storeName);
                throw new OpcSecurityException($"An unexpected error occurred while loading certificate from store '{storeLocation}/{storeName}'.", ex);
            }
            finally
            {
                store?.Close();
            }
        }

         /// <summary>
        /// Loads an X.509 certificate from a specified certificate store path string (e.g., "CurrentUser\My").
        /// </summary>
        /// <param name="storePath">The store path string (e.g., "CurrentUser\My" or "LocalMachine\Root").</param>
        /// <param name="findType">The type of value to find the certificate by.</param>
        /// <param name="findValue">The value to search for.</param>
        /// <param name="validOnly">Specifies whether to return only valid certificates.</param>
        /// <returns>The found X509Certificate2 object, or null if not found.</returns>
        /// <exception cref="ArgumentException">Thrown for invalid store path format.</exception>
        public static X509Certificate2? LoadCertificateFromStore(string storePath, X509FindType findType, object findValue, bool validOnly = false)
        {
            if (string.IsNullOrWhiteSpace(storePath))
            {
                throw new ArgumentException("Store path cannot be null or empty.", nameof(storePath));
            }

            var parts = storePath.Split('\\', '/'); // Allow both path separators
            if (parts.Length != 2)
            {
                throw new ArgumentException($"Invalid store path format: '{storePath}'. Expected 'StoreLocation\\StoreName' (e.g., 'CurrentUser\\My').", nameof(storePath));
            }

            if (!Enum.TryParse<StoreLocation>(parts[0], true, out var storeLocation))
            {
                throw new ArgumentException($"Invalid store location in path: '{parts[0]}'. Valid values are CurrentUser, LocalMachine.", nameof(storePath));
            }

            if (!Enum.TryParse<StoreName>(parts[1], true, out var storeName))
            {
                // For custom store names, this parsing won't work.
                // The X509Store constructor taking string storeName handles custom names for CurrentUser.
                // For this helper, we might restrict to standard StoreName enum values or handle string name differently.
                // This example assumes standard StoreName enum values.
                 _logger?.LogWarning("Could not parse '{StoreNamePart}' as a standard StoreName. Attempting direct use if possible.", parts[1]);
                // Fallback for custom store names (works for CurrentUser location primarily)
                 if (storeLocation == StoreLocation.CurrentUser)
                 {
                     // Use the string directly for custom store name
                     X509Store? store = null;
                     try
                     {
                         store = new X509Store(parts[1], storeLocation); // Use the string part as store name
                         store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                         X509Certificate2Collection foundCertificates = store.Certificates.Find(findType, findValue, validOnly);
                         if (foundCertificates.Count > 0) return new X509Certificate2(foundCertificates[0].Export(X509ContentType.Cert), (string?)null, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                         return null;
                     }
                     finally { store?.Close(); }
                 }
                throw new ArgumentException($"Invalid store name in path: '{parts[1]}'. Refer to System.Security.Cryptography.X509Certificates.StoreName enum.", nameof(storePath));
            }

            return LoadCertificateFromStore(storeName, storeLocation, findType, findValue, validOnly);
        }

        /// <summary>
        /// Basic validation of a certificate (checks expiration and optionally basic chain).
        /// More comprehensive validation (revocation, trust) is handled by the OPC UA stack.
        /// </summary>
        /// <param name="certificate">The certificate to validate.</param>
        /// <param name="checkChain">If true, attempts a basic chain validation.</param>
        /// <returns>True if basic checks pass, false otherwise.</returns>
        public static bool IsCertificateValid(X509Certificate2 certificate, bool checkChain = false)
        {
            if (certificate == null)
            {
                _logger?.LogWarning("Attempted to validate a null certificate.");
                return false;
            }

            // Check expiration
            if (DateTime.Now < certificate.NotBefore || DateTime.Now > certificate.NotAfter)
            {
                _logger?.LogWarning("Certificate '{Subject}' is not valid due to expiration. NotBefore: {NotBefore}, NotAfter: {NotAfter}",
                    certificate.Subject, certificate.NotBefore, certificate.NotAfter);
                return false;
            }

            if (checkChain)
            {
                try
                {
                    X509Chain chain = new X509Chain();
                    // Configure chain policy (e.g., revocation mode, verification flags) if needed
                    // chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    // chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                    bool isValidChain = chain.Build(certificate);
                    if (!isValidChain)
                    {
                        _logger?.LogWarning("Certificate '{Subject}' chain validation failed. Status: {StatusInfo}",
                            certificate.Subject, chain.ChainStatus.FirstOrDefault().StatusInformation?.Trim());
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Exception during certificate chain validation for '{Subject}'.", certificate.Subject);
                    return false;
                }
            }

            _logger?.LogDebug("Certificate '{Subject}' passed basic validation checks.", certificate.Subject);
            return true;
        }
    }
}