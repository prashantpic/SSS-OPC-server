namespace Reporting.Application.Templates.Commands.CreateReportTemplate;

// Using DTOs to decouple application layer from domain value objects in command definitions.

public record DataSourceDto(string Name, string Type, Dictionary<string, string> Parameters);

public record LayoutConfigurationDto(string Header, string Footer);

public record BrandingDto(string? LogoUrl, string? PrimaryColor, string? CompanyName);