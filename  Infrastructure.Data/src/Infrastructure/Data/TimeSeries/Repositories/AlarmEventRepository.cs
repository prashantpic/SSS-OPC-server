using Opc.System.Infrastructure.Data.Abstractions;
using Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.System.Infrastructure.Data.TimeSeries.Repositories
{
    /// <summary>
    /// Placeholder implementation for IAlarmEventRepository.
    /// </summary>
    public class AlarmEventRepository : IAlarmEventRepository
    {
        public Task IngestAlarmEventsAsync(IEnumerable<AlarmEventPoint> alarmEvents, CancellationToken cancellationToken)
        {
            // This is a placeholder for the DI container.
            // A real implementation would be similar to HistoricalDataRepository.
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AlarmEventPoint>> QueryAlarmsAsync(AlarmEventQuery query, CancellationToken cancellationToken)
        {
            // This is a placeholder for the DI container.
            // A real implementation would be similar to HistoricalDataRepository.
            throw new NotImplementedException();
        }
    }
}