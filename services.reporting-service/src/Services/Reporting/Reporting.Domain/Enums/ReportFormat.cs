namespace Reporting.Domain.Enums;

/// <summary>
/// Defines the supported file formats for report generation: PDF, Excel, and HTML.
/// </summary>
public enum ReportFormat
{
    /// <summary>
    /// Portable Document Format.
    /// </summary>
    PDF,

    /// <summary>
    /// Microsoft Excel format.
    /// </summary>
    Excel,

    /// <summary>
    /// HyperText Markup Language format.
    /// </summary>
    HTML
}