using Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels;
// using Opc.System.Domain.Queries; // Assuming AlarmEventQuery is in a domain project
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Placeholder for Domain Query Object
public class AlarmEventQuery { }

namespace Opc.System.Infrastructure.Data.Abstractions
{
    /// <summary>
    /// Provides a contract for interacting with the time-series database for alarm and event logs.
    /// </summary>
    public interface IAlarmEventRepository
    {
        /// <summary>
        /// Writes a batch of alarm and event points to the time-series database.
        /// </summary>
        /// <param name="alarmEvents">The collection of alarm/event points to ingest.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task IngestAlarmEventsAsync(IEnumerable<AlarmEventPoint> alarmEvents, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves alarms based on a flexible query object.
        /// </summary>
        /// <param name="query">The query object specifying filter criteria like time ranges, severity, or acknowledgment state.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A collection of alarm and event points matching the query.</returns>
        Task<IEnumerable<AlarmEventPoint>> QueryAlarmsAsync(AlarmEventQuery query, CancellationToken cancellationToken);
    }
}