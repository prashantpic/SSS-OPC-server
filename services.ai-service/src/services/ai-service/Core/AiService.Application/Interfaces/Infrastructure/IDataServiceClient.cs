namespace AiService.Application.Interfaces.Infrastructure;

/// <summary>
/// Defines the contract for a client that communicates with the Data Service.
/// </summary>
public interface IDataServiceClient
{
    /// <summary>
    /// Fetches data from the Data Service based on a structured NLP result.
    /// </summary>
    /// <param name="nlpResult">The structured result from the NLP service.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, which returns the fetched data as a generic object.</returns>
    Task<object> FetchDataFromNlpResultAsync(StructuredNlpResult nlpResult, CancellationToken cancellationToken = default);
}