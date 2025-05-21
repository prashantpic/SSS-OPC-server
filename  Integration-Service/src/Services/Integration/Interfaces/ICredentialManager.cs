using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    /// <summary>
    /// Interface for managing secure credentials for external system integrations.
    /// Abstracts the retrieval of sensitive credentials (e.g., API keys, tokens, 
    /// certificates, private keys) required for authenticating with IoT platforms, 
    /// Blockchain networks, and Digital Twin services. Implementations could fetch 
    /// credentials from environment variables, configuration files (for dev), 
    /// or a secure vault service.
    /// </summary>
    public interface ICredentialManager
    {
        /// <summary>
        /// Retrieves a credential (e.g., password, API key, token) securely based on its key.
        /// </summary>
        /// <param name="credentialKey">The key or identifier for the credential.</param>
        /// <returns>The credential value as a string.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the credential key is not found.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if credentialKey is null or empty.</exception>
        Task<string> GetCredentialAsync(string credentialKey);

        /// <summary>
        /// Retrieves an X.509 certificate securely based on its identifier (e.g., thumbprint, subject name).
        /// </summary>
        /// <param name="identifier">The identifier for the certificate.</param>
        /// <returns>The X509Certificate2 object.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the certificate is not found.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if identifier is null or empty.</exception>
        Task<X509Certificate2> GetCertificateAsync(string identifier);
    }
}