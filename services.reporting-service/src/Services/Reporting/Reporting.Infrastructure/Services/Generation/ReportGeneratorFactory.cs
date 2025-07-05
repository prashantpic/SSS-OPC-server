using Reporting.Application.Contracts.Generation;
using Reporting.Domain.Enums;

namespace Reporting.Infrastructure.Services.Generation;

/// <summary>
/// Implements the factory for creating report generator instances based on the requested format.
/// </summary>
public class ReportGeneratorFactory : IReportGeneratorFactory
{
    private readonly IReadOnlyDictionary<ReportFormat, IReportGenerator> _generators;

    public ReportGeneratorFactory(IEnumerable<IReportGenerator> generators)
    {
        _generators = generators.ToDictionary(g => g.Format);
    }

    public IReportGenerator Create(ReportFormat format)
    {
        if (_generators.TryGetValue(format, out var generator))
        {
            return generator;
        }

        throw new NotSupportedException($"No report generator is registered for the format '{format}'.");
    }
}