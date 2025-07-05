namespace Reporting.Application.Models;

/// <summary>
/// A model to hold all aggregated data required for generating a single report.
/// </summary>
public class ReportDataModel
{
    public string ReportTitle { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public List<object> DataSections { get; set; } = new();
}