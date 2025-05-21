namespace AIService.Domain.Interfaces
{
    using AIService.Domain.Models;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the contract for an NLP service, abstracting specific providers (spaCy, Azure, Google).
    /// Operations include intent recognition and entity extraction.
    /// (REQ-7-014)
    /// </summary>
    public interface INlpProvider
    {
        /// <summary>
        /// Gets the unique name of this NLP provider (e.g., "SpaCy", "AzureCognitiveServices").
        /// Used for selection based on configuration.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Processes a natural language query to perform tasks like intent recognition and entity extraction.
        /// </summary>
        /// <param name="query">The natural language query text to process.</param>
        /// <param name="context">The NlqContext to populate with processing results. 
        /// This context may already contain some information (e.g., user-defined aliases to consider).</param>
        /// <returns>An updated NlqContext containing the results of the NLP processing.</returns>
        Task<NlqContext> ProcessAsync(string query, NlqContext context);
    }
}