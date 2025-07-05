using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Opc.System.Services.Integration.API.Controllers;

// DTOs / Commands / Queries (can be in separate files in a real project)
public record CreateConnectionCommand(string Name, string ConnectionType, string Endpoint, JsonDocument SecurityConfiguration) : IRequest<Guid>;
public record GetConnectionByIdQuery(Guid Id) : IRequest<ConnectionQueryResult?>;
public record ConnectionQueryResult(Guid Id, string Name, string ConnectionType, string Endpoint, bool IsEnabled, Guid? DataMapId);


/// <summary>
/// Exposes REST endpoints for CRUD operations on IntegrationConnection entities.
/// </summary>
[Authorize(Roles = "Administrator")]
[ApiController]
[Route("api/v1/[controller]")]
public class ConnectionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ConnectionsController> _logger;

    public ConnectionsController(IMediator mediator, ILogger<ConnectionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new integration connection.
    /// </summary>
    /// <param name="command">The command containing connection details.</param>
    /// <returns>The created connection's details.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConnection([FromBody] CreateConnectionCommand command)
    {
        try
        {
            var newId = await _mediator.Send(command);
            _logger.LogInformation("Integration Connection created with ID: {ConnectionId}", newId);
            var query = new GetConnectionByIdQuery(newId);
            var result = await _mediator.Send(query);
            return CreatedAtAction(nameof(GetConnection), new { id = newId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating integration connection.");
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a specific integration connection by its ID.
    /// </summary>
    /// <param name="id">The GUID of the connection.</param>
    /// <returns>The connection details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConnectionQueryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConnection(Guid id)
    {
        var query = new GetConnectionByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            _logger.LogWarning("Integration Connection with ID: {ConnectionId} not found.", id);
            return NotFound();
        }

        return Ok(result);
    }
    
    /// <summary>
    /// Updates an existing integration connection. (Placeholder)
    /// </summary>
    /// <param name="id">The ID of the connection to update.</param>
    /// <param name="command">The update command.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConnection(Guid id, [FromBody] object command)
    {
        // In a real implementation:
        // 1. Create an UpdateConnectionCommand record.
        // 2. Add 'id' to the command.
        // 3. await _mediator.Send(updateCommand);
        // 4. Return NoContent() or NotFound().
        _logger.LogWarning("UpdateConnection endpoint is a placeholder and not implemented.");
        await Task.CompletedTask;
        return NoContent();
    }

    /// <summary>
    /// Deletes an integration connection. (Placeholder)
    /// </summary>
    /// <param name="id">The ID of the connection to delete.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConnection(Guid id)
    {
        // In a real implementation:
        // 1. Create a DeleteConnectionCommand record with the id.
        // 2. await _mediator.Send(deleteCommand);
        // 3. Return NoContent() or NotFound().
        _logger.LogWarning("DeleteConnection endpoint is a placeholder and not implemented.");
        await Task.CompletedTask;
        return NoContent();
    }
}