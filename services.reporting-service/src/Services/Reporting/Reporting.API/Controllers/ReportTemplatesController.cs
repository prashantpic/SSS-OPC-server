using MediatR;
using Microsoft.AspNetCore.Mvc;
using Reporting.Application.Templates.Commands.CreateReportTemplate;

namespace Reporting.API.Controllers;

/// <summary>
/// Handles all incoming HTTP requests related to report templates, acting as the primary interface for template configuration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportTemplatesController : ControllerBase
{
    private readonly ISender _sender;

    public ReportTemplatesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Creates a new report template.
    /// </summary>
    /// <param name="command">The command containing all data for the new template.</param>
    /// <returns>A response with the location of the newly created template.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateReportTemplateCommand command)
    {
        try
        {
            var templateId = await _sender.Send(command);
            return CreatedAtAction(nameof(GetTemplateById), new { id = templateId }, new { id = templateId });
        }
        catch (FluentValidation.ValidationException ex)
        {
            // In a real app, a global exception handler middleware would handle this
            return BadRequest(ex.Errors);
        }
    }
    
    /// <summary>
    /// Retrieves a specific report template by its ID.
    /// </summary>
    /// <param name="id">The GUID of the template to retrieve.</param>
    /// <returns>The report template data.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Replace object with a real Template DTO
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplateById(Guid id)
    {
        // This would use a 'GetReportTemplateByIdQuery'
        // var query = new GetReportTemplateByIdQuery(id);
        // var template = await _sender.Send(query);
        // if (template == null) return NotFound();
        // return Ok(template);
        await Task.CompletedTask;
        return Ok(new { Id = id, Name = "Query Not Implemented" });
    }

    /// <summary>
    /// Retrieves a list of all report templates.
    /// </summary>
    /// <returns>A list of report templates.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)] // Replace object with a real Template DTO
    public async Task<IActionResult> ListTemplates()
    {
        // This would use a 'ListReportTemplatesQuery'
        await Task.CompletedTask;
        return Ok(new List<object>());
    }

    /// <summary>
    /// Updates an existing report template.
    /// </summary>
    /// <param name="id">The ID of the template to update.</param>
    /// <param name="request">The data to update the template with.</param>
    /// <returns>A success response.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] object request) // Replace object with UpdateReportTemplateRequest
    {
        // This would use an 'UpdateReportTemplateCommand'
        await Task.CompletedTask;
        return NoContent();
    }

    /// <summary>
    /// Deletes a report template.
    /// </summary>
    /// <param name="id">The ID of the template to delete.</param>
    /// <returns>A success response.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        // This would use a 'DeleteReportTemplateCommand'
        await Task.CompletedTask;
        return NoContent();
    }
}