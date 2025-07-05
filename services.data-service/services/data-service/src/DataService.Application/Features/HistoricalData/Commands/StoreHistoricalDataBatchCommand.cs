using MediatR;
using System;
using System.Collections.Generic;

namespace DataService.Application.Features.HistoricalData.Commands;

/// <summary>
/// DTO for transferring a single historical data point into the system.
/// </summary>
public record HistoricalDataDto(
    Guid TagId,
    DateTimeOffset Timestamp,
    object Value,
    string Quality);

/// <summary>
/// Represents a command to store a batch of historical data points.
/// This is part of the CQRS pattern and encapsulates the data for the 'Store Historical Data' use case.
/// Fulfills requirement REQ-DLP-001.
/// </summary>
/// <param name="DataPoints">A collection of historical data points to be stored.</param>
public record StoreHistoricalDataBatchCommand(IEnumerable<HistoricalDataDto> DataPoints) : IRequest;