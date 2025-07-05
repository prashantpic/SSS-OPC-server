using ManagementService.Application.Features.BulkOperations;
using ManagementService.Application.Features.ClientLifecycle.Queries;
using ManagementService.Application.Features.ClientManagement.Commands;
using ManagementService.Application.Features.ClientMonitoring;
using ManagementService.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ManagementService.Api.Controllers;

// --- DTOs for API Contracts ---

// Request Bodies
public record UpdateConfigurationRequest(ClientConfigurationDto Configuration);
public record BulkConfigurationRequest(List<Guid> ClientIds, ClientConfigurationDto Configuration);

// Response Bodies
public record ClientSummaryDto(Guid Id, string Name, string Site, string HealthStatus, DateTimeOffset LastSeen);
public record AggregatedKpisDto(int TotalClients, int HealthyClients, int UnhealthyClients, double AverageCpuUsage, double TotalDataThroughput);


/// <summary>
/// The REST API controller for managing and monitoring OPC client instances.
/// This is the primary interface for the centralized management dashboard.
/// </summary>
[ApiController]
[Route("api/clients")]
[Produces("application/json")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of all managed OPC client instances with summary information.
    /// </summary>
    [HttpGet(Name = "GetAllClients")]
    [ProducesResponseType(typeof(List<ClientSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllClients()
    {
        var query = new GetAllClientsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information for a specific client instance.
    /// </summary>
    /// <param name="id">The GUID of the client instance.</param>
    [HttpGet("{id}", Name = "GetClientById")]
    [ProducesResponseType(typeof(ClientDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientById(Guid id)
    {
        var query = new GetClientDetailsQuery(id);
        var result = await _mediator.Send(query);
        return result is not null ? Ok(result) : NotFound();
    }

    /// <summary>
    /// Updates the configuration for a single client instance.
    /// </summary>
    /// <param name="id">The GUID of the client instance to update.</param>
    /// <param name="request">The new configuration.</param>
    [HttpPut("{id}/configuration", Name = "UpdateConfiguration")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateConfiguration(Guid id, [FromBody] UpdateConfigurationRequest request)
    {
        // Map DTO to Domain Value Object for the command
        var domainConfig = new ClientConfiguration(
            request.Configuration.PollingIntervalSeconds,
            request.Configuration.TagConfigurations.Select(t => new TagConfig(t.TagName, t.ScanRate)).ToList()
        );

        var command = new UpdateClientConfigurationCommand(id, domainConfig);
        var result = await _mediator.Send(command);

        return result.IsSuccess ? NoContent() : NotFound();
    }
    
    /// <summary>
    /// Applies a single configuration to multiple client instances simultaneously.
    /// </summary>
    [HttpPut("bulk/configuration", Name = "ExecuteBulkUpdate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ExecuteBulkUpdate([FromBody] BulkConfigurationRequest request)
    {
         var domainConfig = new ClientConfiguration(
            request.Configuration.PollingIntervalSeconds,
            request.Configuration.TagConfigurations.Select(t => new TagConfig(t.TagName, t.ScanRate)).ToList()
        );
        
        var command = new ExecuteBulkConfigurationCommand(request.ClientIds, domainConfig);
        await _mediator.Send(command);
        
        return Accepted();
    }

    /// <summary>
    /// Retrieves aggregated KPIs for the entire fleet of clients.
    /// </summary>
    [HttpGet("kpis/aggregated", Name = "GetAggregatedKpis")]
    [ProducesResponseType(typeof(AggregatedKpisDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAggregatedKpis()
    {
        var query = new GetAggregatedKpisQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}