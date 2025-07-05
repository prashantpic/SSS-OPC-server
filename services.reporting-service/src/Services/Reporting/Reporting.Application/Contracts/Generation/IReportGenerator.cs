using Reporting.Application.Models;
using Reporting.Domain.Enums;

namespace Reporting.Application.Contracts.Generation;

/// <summary>
/// Defines the strategy interface for generating a report in a specific format.
/// </summary>
public interface IReportGenerator
{
    /// <summary>
    /// The report format this generator supports.
    /// </summary>
    ReportFormat Format { get; }

    /// <summary>
    /// Generates a report from the provided data model.
    /// </summary>
    /// <param name="data">The aggregated data to include in the report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A byte array representing the generated report file.</returns>
    Task<byte[]> GenerateAsync(ReportDataModel data, CancellationToken cancellationToken = default);
}