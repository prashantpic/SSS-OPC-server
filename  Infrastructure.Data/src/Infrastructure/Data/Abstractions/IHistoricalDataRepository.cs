using Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels;
// using Opc.System.Domain.Queries; // Assuming HistoricalDataQuery is in a domain project
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Placeholder for Domain Query Object
public class HistoricalDataQuery { }


namespace Opc.System.Infrastructure.Data.Abstractions
{
    /// <summary>
    /// Provides a contract for interacting with the time-series database for historical process values,
    /// abstracting away the specific database technology.
    /// </summary>
    public interface IHistoricalDataRepository
    {
        /// <summary>
        /// Writes a batch of historical data points to the time-series database.
        /// </summary>
        /// <param name="dataBatch">The collection of data points to ingest.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task IngestDataBatchAsync(IEnumerable<HistoricalDataPoint> dataBatch, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves historical data based on a flexible query object.
        /// </summary>
        /// <param name="query">The query object specifying filter criteria like tags, time range, and aggregation.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A collection of historical data points matching the query.</returns>
        Task<IEnumerable<HistoricalDataPoint>> QueryDataAsync(HistoricalDataQuery query, CancellationToken cancellationToken);
    }
}