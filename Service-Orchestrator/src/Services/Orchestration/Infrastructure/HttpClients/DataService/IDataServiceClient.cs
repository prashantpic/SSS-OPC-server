using System.Threading.Tasks;
using OrchestrationService.Workflows.ReportGeneration; // For DTOs like DataFilters, ReportMetadataDto
using OrchestrationService.Workflows.ReportGeneration.Activities; // For HistoricalDataDto
using OrchestrationService.Workflows.BlockchainSync.Activities; // For OffChainStorageResultDto

namespace OrchestrationService.Infrastructure.HttpClients.DataService;

/// <summary>
/// Defines the contract for communication with the external Data Service (REPO-DATA-SERVICE)
/// for operations like historical data retrieval, report archiving, and off-chain data storage.
/// </summary>
public interface IDataServiceClient
{
    /// <summary>
    /// Retrieves historical data based on the provided filters.
    /// </summary>
    /// <param name="filters">Data filtering criteria (e.g., time range, tag IDs).</param>
    /// <returns>A DTO containing a reference (e.g., URI or ID) to the retrieved historical data.</returns>
    Task<HistoricalDataDto> GetHistoricalDataAsync(DataFilters filters);

    /// <summary>
    /// Archives a generated report document.
    /// </summary>
    /// <param name="reportUri">The URI or path of the report document to archive.</param>
    /// <param name="metadata">Metadata associated with the report (e.g., ReportId, Type, Version).</param>
    /// <returns>A DTO containing a reference to the archived report (e.g., archive ID or path).</returns>
    Task<ArchiveReportResponseDto> ArchiveReportAsync(string reportUri, ReportMetadataDto metadata);

    /// <summary>
    /// Stores voluminous data off-chain, typically before committing a hash on-chain.
    /// </summary>
    /// <param name="dataReferenceId">A reference to the original data source or the data payload itself if small.</param>
    /// <param name="dataHash">The cryptographic hash of the data being stored.</param>
    /// <returns>A DTO containing the storage path or reference for the off-chain data.</returns>
    Task<OffChainStorageResultDto> StoreOffChainDataAsync(string dataReferenceId, string dataHash);

    /// <summary>
    /// Marks previously stored off-chain data for cleanup or deletion, used in compensation logic.
    /// </summary>
    /// <param name="offChainStoragePath">The storage path or reference of the off-chain data to mark for cleanup.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task MarkOffChainDataForCleanupAsync(string offChainStoragePath);

    /// <summary>
    /// Deletes a file based on its URI or path. Used for compensation cleanup of generated documents.
    /// </summary>
    /// <param name="fileUri">The URI or path of the file to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string fileUri);
}

// Define ArchiveReportResponseDto if it returns something specific
public class ArchiveReportResponseDto
{
    public string ArchiveReference { get; set; }
    public bool Success { get; set; }
}