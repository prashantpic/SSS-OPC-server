using System.Threading;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    public interface ICredentialManager
    {
        /// <summary>
        /// Retrieves a credential securely.
        /// </summary>
        /// <param name="identifier">The identifier for the credential (e.g., secret name in vault, config key).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The credential value as a string. Implementations should handle secure retrieval and storage.</returns>
        Task<string?> GetCredentialAsync(string identifier, CancellationToken cancellationToken = default);
    }
}