using MediatR;

namespace Reporting.Application.Generation.Commands.GenerateReportOnDemand;

/// <summary>
/// Command object representing the intent to generate a report immediately based on a template.
/// </summary>
/// <param name="TemplateId">The ID of the report template to use.</param>
/// <param name="OutputFormat">The desired output format (e.g., "PDF", "Excel"). Overrides template default.</param>
public record GenerateReportOnDemandCommand(
    Guid TemplateId,
    string OutputFormat
) : IRequest<Guid>;