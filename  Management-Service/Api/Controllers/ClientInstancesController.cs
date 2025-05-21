using MediatR;
using ManagementService.Api.V1.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ManagementService.Application.Features.ClientInstances.Commands.RegisterClientInstance;
using ManagementService.Application.Features.ClientInstances.Commands.UpdateClientStatus;
using ManagementService.Application.Features.ClientInstances.Queries.GetClientInstanceById;
using ManagementService.Application.Features.ClientInstances.Queries.ListClientInstances;
using ManagementService.Application.Features.ClientInstances.Queries.GetAggregatedClientKpis;
using ManagementService.Application.Features.ClientInstances.Queries.GetClientStatus;

namespace ManagementService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientInstancesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientInstancesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new client instance.
    /// REQ-6-001
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> RegisterClientInstance([FromBody] RegisterClientInstanceCommand command)
    {
        var clientId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetClientInstanceById), new { id = clientId }, clientId);
    }

    /// <summary>
    /// Gets details for a specific client instance.
    /// REQ-6-001
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClientInstanceDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetClientInstanceById(Guid id)
    {
        var query = new GetClientInstanceByIdQuery(id);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Gets the current status for a specific client instance.
    /// REQ-6-001
    /// </summary>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(ClientInstanceStatusDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetClientStatus(Guid id)
    {
        var query = new GetClientStatusQuery(id);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }
    
    /// <summary>
    /// Updates the status of a client instance.
    /// REQ-6-001
    /// </summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateClientStatus(Guid id, [FromBody] UpdateClientStatusRequestDto request)
    {
        var command = new UpdateClientStatusCommand(id, request.Status, request.LastSeen ?? DateTimeOffset.UtcNow);
        await _mediator.Send(command);
        return Ok();
    }

    /// <summary>
    /// Lists all registered client instances.
    /// REQ-6-001
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClientInstanceDto>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> ListClientInstances()
    {
        var query = new ListClientInstancesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Gets aggregated KPIs for clients.
    /// REQ-6-002
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(AggregatedKpisDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAggregatedKpis([FromQuery] ClientKpiFilterDto? filter)
    {
        var query = new GetAggregatedClientKpisQuery(filter);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

// Defined here as per SDS structure for this file, normally in DTOs folder.
public record UpdateClientStatusRequestDto(string Status, DateTimeOffset? LastSeen);
public record ClientInstanceStatusDto(Guid Id, string Name, string Status, DateTimeOffset? LastSeen);