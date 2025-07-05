using InfluxDB.Client.Core;
using System;

namespace Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels
{
    /// <summary>
    /// Represents the data transfer object for alarm and event records, tailored for interaction with the InfluxDB time-series database.
    /// Defines the schema for alarm and event records being written to and read from InfluxDB.
    /// </summary>
    [Measurement("alarm_events")]
    public class AlarmEventPoint
    {
        /// <summary>
        /// Gets or sets the source node identifier for the alarm. This is an InfluxDB tag.
        /// </summary>
        [Column("source_node", IsTag = true)]
        public string? SourceNode { get; set; }

        /// <summary>
        /// Gets or sets the type of the event (e.g., HighHigh, Low). This is an InfluxDB tag.
        /// </summary>
        [Column("event_type", IsTag = true)]
        public string? EventType { get; set; }

        /// <summary>
        /// Gets or sets the severity of the alarm. This is an InfluxDB field.
        /// </summary>
        [Column("severity")]
        public int Severity { get; set; }

        /// <summary>
        /// Gets or sets the alarm message. This is an InfluxDB field.
        /// </summary>
        [Column("message")]
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the alarm occurred. This is the InfluxDB timestamp.
        /// </summary>
        [Column(IsTimestamp = true)]
        public DateTime OccurrenceTime { get; set; }
    }
}