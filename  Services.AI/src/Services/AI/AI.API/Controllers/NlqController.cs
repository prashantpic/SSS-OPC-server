using MediatR;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.AI.Application.Features.Nlq;

namespace Opc.System.Services.AI.API.Controllers;

#region Request DTOs
// DTOs are defined here to satisfy dependencies without creating unlisted files.
public record NlqRequestDto(string Query);
public record CreateNlqAliasDto(string Alias, string TagId);
// The command for creating an alias would be defined in the application layer.
public record CreateNlqAliasCommand(string Alias, string TagId) : IRequest;
#endregion


/// <summary>
/// REST API controller for processing Natural Language Queries.
/// </summary>
[ApiController]
[Route("api/nlq")]
public class NlqController : ControllerBase
{
    private readonly ISender _mediator;

    public NlqController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Processes a user-submitted natural language query.
    /// </summary>
    /// <param name="request">The request containing the query text.</param>
    /// <returns>The result of the query.</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(NlqResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessQuery([FromBody] NlqRequestDto request)
    {
        var command = new ProcessNlqCommand(request.Query);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Creates or updates an alias for an OPC tag to improve NLQ accuracy.
    /// </summary>
    /// <param name="request">The request containing the alias and the target tag ID.</param>
    /// <returns>A confirmation of the alias creation.</returns>
    [HttpPost("alias")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAlias([FromBody] CreateNlqAliasDto request)
    {
        // The handler for this command needs to be implemented in the Application layer.
        var command = new CreateNlqAliasCommand(request.Alias, request.TagId);
        await _mediator.Send(command);
        return StatusCode(StatusCodes.Status201Created);
    }
}