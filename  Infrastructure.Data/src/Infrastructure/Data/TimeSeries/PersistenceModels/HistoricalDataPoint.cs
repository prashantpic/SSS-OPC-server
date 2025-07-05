using InfluxDB.Client.Core;
using System;

namespace Opc.System.Infrastructure.Data.TimeSeries.PersistenceModels
{
    /// <summary>
    /// Represents the data transfer object for historical data points, tailored for interaction with the InfluxDB time-series database.
    /// Defines the schema for data points being written to and read from InfluxDB.
    /// </summary>
    [Measurement("historical_data")]
    public class HistoricalDataPoint
    {
        /// <summary>
        /// Gets or sets the ID of the tag this data point belongs to. This is an InfluxDB tag.
        /// </summary>
        [Column("tagId", IsTag = true)]
        public string? TagId { get; set; }

        /// <summary>
        /// Gets or sets the value of the data point. This is an InfluxDB field.
        /// </summary>
        [Column("value")]
        public object? Value { get; set; }

        /// <summary>
        /// Gets or sets the quality of the data point. This is an InfluxDB field.
        /// </summary>
        [Column("quality")]
        public string? Quality { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the data point. This is the InfluxDB timestamp.
        /// </summary>
        [Column(IsTimestamp = true)]
        public DateTime Timestamp { get; set; }
    }
}