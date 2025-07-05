using Reporting.Domain.Enums;

namespace Reporting.Application.Contracts.Generation;

/// <summary>
/// Defines the factory interface for creating report generator instances.
/// </summary>
public interface IReportGeneratorFactory
{
    /// <summary>
    /// Creates an IReportGenerator instance for the specified format.
    /// </summary>
    /// <param name="format">The desired report format.</param>
    /// <returns>An instance of IReportGenerator.</returns>
    IReportGenerator Create(ReportFormat format);
}