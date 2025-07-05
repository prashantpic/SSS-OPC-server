using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataService.Api.Controllers;

// DTO and Query defined here for clarity, will be moved to Application layer in a subsequent step.
public record HistoricalDataDto(Guid TagId, DateTimeOffset Timestamp, object Value, string Quality);
public record GetHistoricalDataQuery(Guid TagId, DateTimeOffset StartTime, DateTimeOffset EndTime, string? Aggregate) : IRequest<IEnumerable<HistoricalDataDto>>;


/// <summary>
/// API controller for handling requests related to historical data.
/// Fulfills requirement REQ-DLP-001.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HistoricalDataController : ControllerBase
{
    private readonly ISender _mediator;

    public HistoricalDataController(ISender mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets historical data for a specific tag within a time range.
    /// </summary>
    /// <param name="query">The query parameters including TagId, StartTime, EndTime, and an optional Aggregate function.</param>
    /// <returns>A collection of historical data points.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HistoricalDataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetHistoricalData([FromQuery] GetHistoricalDataQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}