namespace Reporting.Domain.ValueObjects;

/// <summary>
/// Represents the branding configuration for a report.
/// </summary>
/// <param name="LogoUrl">The URL of the company logo.</param>
/// <param name="PrimaryColor">The primary branding color (e.g., in hex format #RRGGBB).</param>
/// <param name="CompanyName">The name of the company to display in the report.</param>
public record Branding(
    string? LogoUrl,
    string? PrimaryColor,
    string? CompanyName);