using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reporting.Application.Generation.Commands.GenerateReportOnDemand;

namespace Reporting.API.Controllers;

/// <summary>
/// Handles all incoming HTTP requests related to the generation and retrieval of specific report instances.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Triggers an on-demand report generation job.
    /// </summary>
    /// <param name="request">The request containing the template ID and desired output format.</param>
    /// <returns>An Accepted response with a link to check the status.</returns>
    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GenerateReportOnDemand([FromBody] GenerateReportOnDemandRequest request)
    {
        var command = new GenerateReportOnDemandCommand(request.TemplateId, request.OutputFormat);
        var reportId = await _sender.Send(command);

        // Return 202 Accepted, indicating the request has been accepted for processing.
        // The Location header points to the status endpoint.
        return AcceptedAtAction(nameof(GetReportStatus), new { reportId = reportId }, new { reportId });
    }

    /// <summary>
    /// Gets the status of a report generation job.
    /// </summary>
    /// <param name="reportId">The unique ID of the generated report.</param>
    /// <returns>The current status of the report.</returns>
    [HttpGet("{reportId:guid}/status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Replace object with a real Status DTO
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportStatus(Guid reportId)
    {
        // In a full implementation, this would send a query to get the report status.
        // var query = new GetReportStatusQuery(reportId);
        // var status = await _sender.Send(query);
        // return Ok(status);

        // For now, return a placeholder
        await Task.CompletedTask;
        return Ok(new { ReportId = reportId, Status = "Query not implemented. Use this ID for download." });
    }

    /// <summary>
    /// Downloads a completed report file.
    /// </summary>
    /// <param name="reportId">The unique ID of the generated report.</param>
    /// <returns>The generated file or an appropriate error response.</returns>
    [HttpGet("{reportId:guid}/download")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // If report is still processing
    public async Task<IActionResult> DownloadReport(Guid reportId)
    {
        // In a full implementation, this would:
        // 1. Send a query to get the GeneratedReport entity.
        // 2. Check its status. If not 'Completed', return 409 or 404.
        // 3. If 'Completed', read the file from the path stored in the entity.
        // 4. Return a FileResult with the correct content type.

        await Task.CompletedTask;
        return NotFound("Download functionality not fully implemented.");
    }
}

/// <summary>
/// DTO for the on-demand generation request body.
/// </summary>
public record GenerateReportOnDemandRequest(Guid TemplateId, string OutputFormat);