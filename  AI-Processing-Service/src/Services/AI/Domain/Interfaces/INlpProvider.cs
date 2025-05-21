using AIService.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AIService.Domain.Interfaces
{
    /// <summary>
    /// Defines the contract for an NLP service, abstracting specific providers (spaCy, Azure, Google).
    /// Operations include intent recognition and entity extraction.
    /// </summary>
    public interface INlpProvider
    {
        /// <summary>
        /// Gets the name of the NLP provider (e.g., "SpaCy", "AzureCognitiveServices").
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Processes the given query text to extract intent, entities, and other relevant NLP information.
        /// </summary>
        /// <param name="query">The natural language query text.</param>
        /// <param name="context">The NlqContext object to be populated with processing results.
        /// It may contain initial context like user-defined aliases.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The updated NlqContext with NLP processing results.</returns>
        Task<NlqContext> ProcessAsync(string query, NlqContext context, CancellationToken cancellationToken = default);
    }
}