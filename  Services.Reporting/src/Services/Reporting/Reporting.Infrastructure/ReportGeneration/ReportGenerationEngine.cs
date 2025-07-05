using Microsoft.Extensions.Logging;
using Opc.System.Services.Reporting.Application.Abstractions;
using Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport;
using Opc.System.Services.Reporting.Application.Models;

namespace Opc.System.Services.Reporting.Infrastructure.ReportGeneration;

/// <summary>
/// Central component for report generation. It selects the appropriate format-specific generator (PDF, Excel, etc.)
/// and invokes it to create the report file stream.
/// To act as a factory or strategy selector for the different concrete report format generators (PDF, Excel, HTML).
/// </summary>
public class ReportGenerationEngine : IReportGenerationEngine
{
    private readonly IEnumerable<IReportFormatGenerator> _generators;
    private readonly ILogger<ReportGenerationEngine> _logger;

    public ReportGenerationEngine(IEnumerable<IReportFormatGenerator> generators, ILogger<ReportGenerationEngine> logger)
    {
        _generators = generators;
        _logger = logger;
    }

    /// <summary>
    /// Generates a report file stream based on the provided data and format.
    /// </summary>
    /// <param name="data">The aggregated data for the report.</param>
    /// <param name="format">The desired output format (e.g., PDF, Excel).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the generated report file.</returns>
    /// <exception cref="NotSupportedException">Thrown if no generator is found for the requested format.</exception>
    public Task<Stream> GenerateAsync(ReportDataModel data, ReportFormat format, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to generate report in {Format} format.", format);

        var generator = _generators.FirstOrDefault(g => g.Format == format);

        if (generator is null)
        {
            _logger.LogError("No report generator found for format {Format}", format);
            throw new NotSupportedException($"Report format '{format}' is not supported.");
        }

        _logger.LogDebug("Using generator {GeneratorType} for format {Format}", generator.GetType().Name, format);
        
        // Assuming generators are synchronous as per their simple spec
        var stream = generator.Generate(data);
        
        return Task.FromResult(stream);
    }
}