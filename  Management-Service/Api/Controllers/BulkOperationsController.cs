using MediatR;
using ManagementService.Api.V1.DTOs;
using ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkConfiguration;
using ManagementService.Application.Features.BulkOperations.Commands.InitiateBulkUpdate;
using ManagementService.Application.Features.BulkOperations.Queries.GetBulkOperationJobStatus;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ManagementService.Api.Controllers;

[ApiController]
[Route("api/v1/bulk")]
public class BulkOperationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BulkOperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Initiates a bulk configuration deployment to multiple client instances.
    /// REQ-6-002
    /// </summary>
    [HttpPost("configurations")]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> InitiateBulkConfiguration([FromBody] BulkConfigurationRequestDto request)
    {
        var command = new InitiateBulkConfigurationCommand(request.ClientInstanceIds, request.ConfigurationVersionId);
        var jobId = await _mediator.Send(command);
        return AcceptedAtAction(nameof(GetBulkOperationStatus), new { jobId = jobId }, jobId);
    }

    /// <summary>
    /// Initiates a bulk software update for multiple client instances.
    /// REQ-6-002
    /// </summary>
    [HttpPost("updates")]
    [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Accepted)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> InitiateBulkUpdate([FromBody] BulkUpdateRequestDto request)
    {
        var command = new InitiateBulkUpdateCommand(request.ClientInstanceIds, request.UpdatePackageUrl, request.TargetVersion);
        var jobId = await _mediator.Send(command);
        return AcceptedAtAction(nameof(GetBulkOperationStatus), new { jobId = jobId }, jobId);
    }

    /// <summary>
    /// Gets the status of a bulk operation job.
    /// REQ-6-002
    /// </summary>
    [HttpGet("{jobId}/status")]
    [ProducesResponseType(typeof(BulkOperationJobDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetBulkOperationStatus(Guid jobId)
    {
        var query = new GetBulkOperationJobStatusQuery(jobId);
        var result = await _mediator.Send(query);
        return result != null ? Ok(result) : NotFound();
    }
}