using AiService.Api.Dtos;
using AiService.Application.Dtos;
using AiService.Application.Features.Nlq.Queries.ProcessNlq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiService.Api.Controllers;

/// <summary>
/// Exposes the Natural Language Query feature via a RESTful API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NlqController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<NlqController> _logger;

    public NlqController(ISender sender, ILogger<NlqController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Submits a natural language query for processing.
    /// </summary>
    /// <param name="requestDto">The DTO containing the query text.</param>
    /// <returns>A structured result based on the interpretation of the query.</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(NlqResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessQuery([FromBody] NlqRequestDto requestDto)
    {
        // In a real application with authentication, the user ID would come from the token claims.
        // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // var userId = Guid.TryParse(userIdString, out var id) ? id : Guid.Empty;
        var userId = Guid.NewGuid(); // Placeholder for anonymous access or testing
        _logger.LogInformation("Received NLQ request from user {UserId}", userId);
        
        var query = new ProcessNlqQuery(requestDto.QueryText, userId);
        var result = await _sender.Send(query);
        
        return Ok(result);
    }
}