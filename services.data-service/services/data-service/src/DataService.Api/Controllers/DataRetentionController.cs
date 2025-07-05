using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataService.Api.Controllers;

// DTO, Command and Query defined here for clarity, will be moved to Application layer in a subsequent step.
public record DataRetentionPolicyDto(Guid Id, string DataType, int RetentionPeriodDays, string Action, bool IsActive, string? ArchiveLocation);
public record GetPoliciesQuery() : IRequest<IEnumerable<DataRetentionPolicyDto>>;
public record UpdatePolicyCommand(Guid? PolicyId, string DataType, int RetentionDays, string Action, bool IsActive, string? ArchiveLocation) : IRequest<Guid>;


/// <summary>
/// API controller for managing data retention policies.
/// Fulfills requirement REQ-DLP-017.
/// </summary>
[ApiController]
[Route("api/retention-policies")]
public class DataRetentionController : ControllerBase
{
    private readonly ISender _mediator;

    public DataRetentionController(ISender mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets all configured data retention policies.
    /// </summary>
    /// <returns>A list of data retention policies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DataRetentionPolicyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPolicies()
    {
        var query = new GetPoliciesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new data retention policy.
    /// </summary>
    /// <param name="command">The command to create the policy. The PolicyId should be null.</param>
    /// <returns>A 201 Created response with the location of the new resource.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DataRetentionPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePolicy([FromBody] UpdatePolicyCommand command)
    {
        if (command.PolicyId.HasValue)
        {
            return BadRequest("PolicyId must be null for creation.");
        }
        var newPolicyId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPolicies), new { id = newPolicyId }, null);
    }
    
    /// <summary>
    /// Updates an existing data retention policy.
    /// </summary>
    /// <param name="id">The ID of the policy to update.</param>
    /// <param name="command">The command with the updated policy data.</param>
    /// <returns>An HTTP 204 No Content response on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdatePolicyCommand command)
    {
        if (command.PolicyId.HasValue && command.PolicyId != id)
        {
            return BadRequest("Policy ID in URL and body do not match.");
        }
        var commandWithId = command with { PolicyId = id };
        await _mediator.Send(commandWithId);
        return NoContent();
    }
}