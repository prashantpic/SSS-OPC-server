using MediatR;

namespace Reporting.Application.Templates.Commands.CreateReportTemplate;

/// <summary>
/// Command object representing the intent to create a new report template.
/// Contains all necessary data for creation.
/// </summary>
public record CreateReportTemplateCommand(
    string Name,
    List<DataSourceDto> DataSources,
    LayoutConfigurationDto Layout,
    BrandingDto Branding,
    string DefaultFormat,
    string? Schedule) : IRequest<Guid>;