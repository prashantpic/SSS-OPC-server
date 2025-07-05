using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport;
using Opc.System.Services.Reporting.Application.Features.Reports.Commands.SignOff;
using Opc.System.Services.Reporting.Application.Features.Reports.Queries;

namespace Opc.System.Services.Reporting.API.Controllers;

/// <summary>
/// Exposes HTTP endpoints for on-demand report generation, status checking, downloading completed reports, and managing sign-off workflows.
/// </summary>
[ApiController]
[Route("api/reports")]
[Authorize] // All endpoints require authentication by default
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Triggers an on-demand generation of a report from a template.
    /// </summary>
    /// <param name="command">The command specifying the template and format.</param>
    /// <returns>An Accepted response with a link to the status endpoint.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateReportOnDemand([FromBody] GenerateReportCommand command)
    {
        var reportId = await _sender.Send(command);
        return AcceptedAtAction(nameof(GetReportStatus), new { reportId }, new { reportId });
    }

    /// <summary>
    /// Gets the status of a generated report.
    /// </summary>
    /// <param name="reportId">The unique identifier of the generated report.</param>
    /// <returns>The status of the report generation process.</returns>
    [HttpGet("{reportId:guid}/status")]
    [ProducesResponseType(typeof(GeneratedReportStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportStatus(Guid reportId)
    {
        var query = new GetGeneratedReportStatusQuery(reportId);
        var result = await _sender.Send(query);
        return result is not null ? Ok(result) : NotFound();
    }
    
    /// <summary>
    /// Downloads the file for a completed report.
    /// </summary>
    /// <param name="reportId">The unique identifier of the generated report.</param>
    /// <returns>A file stream result for the report.</returns>
    [HttpGet("{reportId:guid}/download")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadReport(Guid reportId)
    {
        var query = new GetGeneratedReportFileQuery(reportId);
        var result = await _sender.Send(query);
        
        if(result is null || result.FileStream is null)
        {
            return NotFound("Report not found or not yet completed.");
        }

        return File(result.FileStream, result.ContentType, result.FileName);
    }
    
    /// <summary>
    /// Approves a report as part of the sign-off workflow.
    /// </summary>
    /// <param name="reportId">The unique identifier of the generated report.</param>
    /// <returns>An OK response if the approval was successful.</returns>
    [HttpPost("{reportId:guid}/approve")]
    [Authorize(Policy = "CanApproveReports")] // Example of policy-based authorization
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApproveReport(Guid reportId)
    {
        // In a real app, the user ID would come from the JWT claims.
        var userId = User.Identity?.Name ?? "unknown.user";
        
        var command = new ApproveReportSignOffCommand(reportId, userId);
        await _sender.Send(command);
        return NoContent();
    }
}