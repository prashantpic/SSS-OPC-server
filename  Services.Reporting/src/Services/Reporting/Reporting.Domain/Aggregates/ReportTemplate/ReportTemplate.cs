using Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate.ValueObjects;
using Opc.System.Services.Reporting.Domain.Common;

namespace Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate;

/// <summary>
/// Defines the structure and behavior of a report template, including its customizable elements like data sources, KPIs, charts, and branding.
/// Represents the core domain model for a report template, enforcing consistency and business rules for its configuration.
/// </summary>
public class ReportTemplate : AggregateRoot<Guid>
{
    private readonly List<DataSource> _dataSources = new();
    private readonly List<KpiDefinition> _kpiDefinitions = new();
    private readonly List<ChartConfiguration> _chartConfigurations = new();
    private readonly List<DistributionTarget> _distributionTargets = new();

    /// <summary>
    /// The user-defined name of the template.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// A value object for logos and color schemes.
    /// </summary>
    public Branding Branding { get; private set; }

    /// <summary>
    /// A collection of data sources for the report.
    /// </summary>
    public IReadOnlyCollection<DataSource> DataSources => _dataSources.AsReadOnly();
    
    /// <summary>
    /// A collection of KPI calculation rules.
    /// </summary>
    public IReadOnlyCollection<KpiDefinition> KpiDefinitions => _kpiDefinitions.AsReadOnly();

    /// <summary>
    /// A collection of chart definitions.
    /// </summary>
    public IReadOnlyCollection<ChartConfiguration> ChartConfigurations => _chartConfigurations.AsReadOnly();

    /// <summary>
    /// Default recipients of the report.
    /// </summary>
    public IReadOnlyCollection<DistributionTarget> DefaultDistributionTargets => _distributionTargets.AsReadOnly();

    private ReportTemplate(Guid id, string name, Branding branding) : base(id)
    {
        Name = name;
        Branding = branding;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    // Required for EF Core
    private ReportTemplate() : base(Guid.NewGuid()) { }
#pragma warning restore CS8618

    /// <summary>
    /// Static factory method to ensure valid initial state.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="branding">The branding configuration for the template.</param>
    /// <returns>A new instance of ReportTemplate.</returns>
    public static ReportTemplate Create(string name, Branding branding)
    {
        // Add validation logic here (e.g., Guard clauses)
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name cannot be empty.", nameof(name));
        }

        return new ReportTemplate(Guid.NewGuid(), name, branding);
    }

    /// <summary>
    /// Updates basic properties of the template.
    /// </summary>
    /// <param name="newName">The new name for the template.</param>
    /// <param name="newBranding">The new branding configuration.</param>
    public void UpdateDetails(string newName, Branding newBranding)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Template name cannot be empty.", nameof(newName));
        }

        Name = newName;
        Branding = newBranding;
    }

    /// <summary>
    /// Adds a data source, performing validation (e.g., preventing duplicates).
    /// </summary>
    /// <param name="dataSource">The data source to add.</param>
    public void AddDataSource(DataSource dataSource)
    {
        if (_dataSources.Any(ds => ds.SourceType == dataSource.SourceType && ds.Parameters.SequenceEqual(dataSource.Parameters)))
        {
            // Or throw a custom domain exception
            return; 
        }
        _dataSources.Add(dataSource);
    }
    
    /// <summary>
    /// Removes a chart configuration from the template.
    /// </summary>
    /// <param name="chartId">The ID of the chart to remove.</param>
    public void RemoveChartConfiguration(Guid chartId)
    {
        var chartToRemove = _chartConfigurations.FirstOrDefault(c => c.Id == chartId);
        if (chartToRemove != null)
        {
            _chartConfigurations.Remove(chartToRemove);
        }
    }

    /// <summary>
    /// Modifies a chart configuration.
    /// </summary>
    /// <param name="newConfig">The updated chart configuration.</param>
    public void UpdateChartConfiguration(ChartConfiguration newConfig)
    {
        var existingChart = _chartConfigurations.FirstOrDefault(c => c.Id == newConfig.Id);
        if (existingChart != null)
        {
            _chartConfigurations.Remove(existingChart);
            _chartConfigurations.Add(newConfig);
        }
        else
        {
            _chartConfigurations.Add(newConfig);
        }
    }
    
    /// <summary>
    /// Adds a default distribution target.
    /// </summary>
    /// <param name="target">The distribution target to add.</param>
    public void AddDistributionTarget(DistributionTarget target)
    {
        if (!_distributionTargets.Contains(target))
        {
            _distributionTargets.Add(target);
        }
    }
}