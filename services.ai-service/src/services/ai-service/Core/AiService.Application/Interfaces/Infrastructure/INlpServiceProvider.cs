namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Defines the contract for an external Natural Language Processing service provider.
/// </summary>
public interface INlpServiceProvider
{
    /// <summary>
    /// Sends a natural language query to the provider and gets a structured interpretation.
    /// </summary>
    /// <param name="queryText">The user's query in plain text.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, which returns the structured NLP result.</returns>
    Task<StructuredNlpResult> InterpretQueryAsync(string queryText, CancellationToken cancellationToken = default);
}