using DataService.Domain.Entities;
using DataService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataService.Application.Features.HistoricalData.Commands;

/// <summary>
/// Handles the StoreHistoricalDataBatchCommand, orchestrating the persistence of historical data.
/// </summary>
public class StoreHistoricalDataBatchCommandHandler : IRequestHandler<StoreHistoricalDataBatchCommand>
{
    private readonly ITimeSeriesRepository _timeSeriesRepository;
    private readonly ILogger<StoreHistoricalDataBatchCommandHandler> _logger;

    public StoreHistoricalDataBatchCommandHandler(
        ITimeSeriesRepository timeSeriesRepository,
        ILogger<StoreHistoricalDataBatchCommandHandler> logger)
    {
        _timeSeriesRepository = timeSeriesRepository ?? throw new ArgumentNullException(nameof(timeSeriesRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes the command to store a batch of historical data.
    /// </summary>
    /// <param name="request">The command containing the data points.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Task representing the completion of the operation.</returns>
    public async Task Handle(StoreHistoricalDataBatchCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling StoreHistoricalDataBatchCommand for {Count} data points.", request.DataPoints.Count());

        try
        {
            var domainDataPoints = request.DataPoints.Select(dto => new HistoricalDataPoint
            {
                // The 'Measurement' acts like a table name in InfluxDB.
                Measurement = "opc-data",
                TagId = dto.TagId,
                Timestamp = dto.Timestamp,
                Value = dto.Value,
                Quality = dto.Quality,
                // InfluxDB tags are indexed and used for filtering. TagId is a good candidate.
                Tags = new Dictionary<string, string>
                {
                    { "tagId", dto.TagId.ToString() }
                }
            }).ToList();

            if (!domainDataPoints.Any())
            {
                _logger.LogWarning("Command contained no data points after mapping. Aborting.");
                return;
            }

            await _timeSeriesRepository.AddHistoricalDataBatchAsync(domainDataPoints, cancellationToken);

            _logger.LogInformation("Successfully persisted {Count} historical data points.", domainDataPoints.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store historical data batch.");
            // Re-throwing allows the caller (e.g., message consumer) to handle the failure,
            // for example by sending the message to a dead-letter queue.
            throw;
        }
    }
}