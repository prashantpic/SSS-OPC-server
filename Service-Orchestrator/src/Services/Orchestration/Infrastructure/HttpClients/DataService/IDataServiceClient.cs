using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OrchestrationService.Infrastructure.HttpClients.DataService
{
    // Placeholder DTOs
    public record DataQueryParametersDto(string QueryType, Dictionary<string, object> Filters);
    public record HistoricalDataResultDto(string DataReference, int RecordCount); // Simplified

    /// <summary>
    /// Defines the contract for communication with the external Data Service
    /// for operations like historical data retrieval, report archiving, and off-chain data storage.
    /// Implements REQ-7-020, REQ-7-022, REQ-8-007, REQ-DLP-025.
    /// </summary>
    public interface IDataServiceClient
    {
        /// <summary>
        /// Queries historical data from the Data Service.
        /// </summary>
        /// <param name="parameters">Parameters for the data query.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A reference or summary of the retrieved historical data.</returns>
        Task<HistoricalDataResultDto> QueryHistoricalDataAsync(DataQueryParametersDto parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Archives a report document in the Data Service.
        /// </summary>
        /// <param name="reportId">The ID of the report.</param>
        /// <param name="documentUri">The URI of the document to archive.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation. Optionally, a URI to the archived report.</returns>
        Task<string?> ArchiveReportAsync(string reportId, string documentUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stores voluminous data off-chain.
        /// </summary>
        /// <param name="data">The data payload to store.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A reference or URI to the stored off-chain data.</returns>
        Task<string> StoreOffChainDataAsync(byte[] data, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a document, typically used for compensation logic.
        /// </summary>
        /// <param name="documentUri">The URI of the document to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteDocumentAsync(string documentUri, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks off-chain stored data as 'failed-sync' or schedules for cleanup.
        /// Used in compensation for blockchain sync failures.
        /// </summary>
        /// <param name="offChainStoragePath">The path or reference to the off-chain data.</param>
        /// <param name="reason">Reason for failure.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task MarkOffChainDataForCleanupAsync(string offChainStoragePath, string reason, CancellationToken cancellationToken = default);
    }
}