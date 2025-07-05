using DataService.Domain.Entities;
using DataService.Domain.Repositories;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace DataService.Infrastructure.Persistence.TimeSeries;

// Placeholder for settings class, typically in a separate file
public class InfluxDbSettings
{
    public string Url { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Org { get; set; } = string.Empty;
    public string BucketHistorical { get; set; } = string.Empty;
    public string BucketAlarms { get; set; } = string.Empty;
}

/// <summary>
/// Concrete implementation of ITimeSeriesRepository using the InfluxDB C# client.
/// This class handles all direct interactions with the InfluxDB instance.
/// </summary>
public class TimeSeriesRepository : ITimeSeriesRepository
{
    private readonly IInfluxDBClient _influxDbClient;
    private readonly InfluxDbSettings _settings;
    private readonly ILogger<TimeSeriesRepository> _logger;

    public TimeSeriesRepository(IInfluxDBClient influxDbClient, IOptions<InfluxDbSettings> settings, ILogger<TimeSeriesRepository> logger)
    {
        _influxDbClient = influxDbClient ?? throw new ArgumentNullException(nameof(influxDbClient));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task AddHistoricalDataBatchAsync(IEnumerable<HistoricalDataPoint> dataPoints, CancellationToken cancellationToken)
    {
        using var writeApi = _influxDbClient.GetWriteApiAsync();

        var points = dataPoints.Select(dp =>
        {
            var point = PointData
                .Measurement(dp.Measurement)
                .Timestamp(dp.Timestamp.UtcDateTime, WritePrecision.Ns)
                .Field("value", dp.Value)
                .Field("quality", dp.Quality);

            foreach (var tag in dp.Tags)
            {
                point.Tag(tag.Key, tag.Value);
            }
            return point;
        }).ToList();
        
        _logger.LogDebug("Writing {Count} points to InfluxDB bucket {Bucket}", points.Count, _settings.BucketHistorical);

        await writeApi.WritePointsAsync(points, _settings.BucketHistorical, _settings.Org, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<HistoricalDataPoint>> QueryHistoricalDataAsync(Guid tagId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
    {
        var fluxQuery = new StringBuilder();
        fluxQuery.AppendLine($"from(bucket: \"{_settings.BucketHistorical}\")");
        fluxQuery.AppendLine($"|> range(start: {start:o}, stop: {end:o})");
        fluxQuery.AppendLine($"|> filter(fn: (r) => r._measurement == \"opc-data\")");
        fluxQuery.AppendLine($"|> filter(fn: (r) => r.tagId == \"{tagId}\")");
        fluxQuery.AppendLine($"|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")"); // Make fields into columns

        _logger.LogDebug("Executing Flux query: \n{Query}", fluxQuery.ToString());

        var queryApi = _influxDbClient.GetQueryApi();
        var tables = await queryApi.QueryAsync(fluxQuery.ToString(), _settings.Org, cancellationToken);

        var results = new List<HistoricalDataPoint>();
        foreach (var record in tables.SelectMany(table => table.Records))
        {
            results.Add(new HistoricalDataPoint
            {
                Measurement = record.GetMeasurement(),
                Timestamp = record.GetTimeInDateTimeOffset().GetValueOrDefault(),
                TagId = Guid.Parse(record.GetValueByKey("tagId").ToString()!),
                Value = record.GetValueByKey("value"),
                Quality = record.GetValueByKey("quality").ToString()!,
                Tags = new Dictionary<string, string> { { "tagId", tagId.ToString() } }
            });
        }
        
        return results;
    }

    /// <inheritdoc />
    public async Task DeleteDataBeforeAsync(string bucket, DateTimeOffset cutoff, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to delete data from bucket '{Bucket}' before {CutoffTime}", bucket, cutoff);

        var deleteApi = _influxDbClient.GetDeleteApi();
        
        // InfluxDB requires a predicate, but it can be an empty string to delete all in the time range.
        var predicate = ""; 
        var startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        try
        {
            await deleteApi.Delete(startTime, cutoff.UtcDateTime, predicate, bucket, _settings.Org, cancellationToken);
            _logger.LogInformation("Successfully submitted delete request for bucket '{Bucket}'", bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete data from bucket '{Bucket}'", bucket);
            throw;
        }
    }
}