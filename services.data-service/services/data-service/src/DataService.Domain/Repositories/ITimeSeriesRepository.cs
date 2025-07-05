using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DataService.Domain.Entities;

namespace DataService.Domain.Repositories
{
    /// <summary>
    /// Defines the contract for a repository that handles persistence operations for time-series data.
    /// This interface abstracts the specific time-series database technology (e.g., InfluxDB, TimescaleDB),
    /// allowing the application layer to remain persistence-ignorant.
    /// Fulfills requirement REQ-DLP-001.
    /// </summary>
    public interface ITimeSeriesRepository
    {
        /// <summary>
        /// Writes a batch of historical data points to the time-series store.
        /// </summary>
        /// <param name="dataPoints">The collection of data points to write.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddHistoricalDataBatchAsync(IEnumerable<HistoricalDataPoint> dataPoints, CancellationToken cancellationToken);

        /// <summary>
        /// Queries historical data for a specific tag within a given time range.
        /// </summary>
        /// <param name="tagId">The unique identifier of the tag to query.</param>
        /// <param name="start">The start of the time range.</param>
        /// <param name="end">The end of the time range.</param>
        /// <returns>A collection of historical data points matching the criteria.</returns>
        Task<IEnumerable<HistoricalDataPoint>> QueryHistoricalDataAsync(Guid tagId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes data older than a specified cutoff timestamp from a given bucket/table.
        /// This is used by the DataRetentionWorker.
        /// </summary>
        /// <param name="bucket">The target bucket or table name.</param>
        /// <param name="cutoff">The timestamp before which data should be deleted.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteDataBeforeAsync(string bucket, DateTimeOffset cutoff, CancellationToken cancellationToken);
    }
}