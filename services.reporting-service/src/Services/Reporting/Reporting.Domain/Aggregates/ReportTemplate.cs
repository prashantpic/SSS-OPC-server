using Reporting.Domain.Entities.Base;
using Reporting.Domain.Enums;
using Reporting.Domain.ValueObjects;

namespace Reporting.Domain.Aggregates;

/// <summary>
/// Represents the core business entity for a report template, containing all its configuration and business rules.
/// This is an aggregate root.
/// </summary>
public class ReportTemplate : Entity<Guid>
{
    public string Name { get; private set; }
    public Branding Branding { get; private set; }
    public ReportFormat DefaultFormat { get; private set; }
    public Schedule? Schedule { get; private set; }

    private readonly List<DataSource> _dataSources = new();
    public IReadOnlyCollection<DataSource> DataSources => _dataSources.AsReadOnly();

    // Private constructor for EF Core and factory method
    private ReportTemplate() { }

    private ReportTemplate(Guid id, string name, Branding branding, ReportFormat defaultFormat, List<DataSource> dataSources, Schedule? schedule) : base(id)
    {
        Name = name;
        Branding = branding;
        DefaultFormat = defaultFormat;
        Schedule = schedule;
        _dataSources.AddRange(dataSources);
    }

    /// <summary>
    /// Factory method to create a new ReportTemplate.
    /// </summary>
    public static ReportTemplate Create(Guid id, string name, Branding branding, ReportFormat defaultFormat, List<DataSource> dataSources, Schedule? schedule)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name cannot be null or whitespace.", nameof(name));

        if (dataSources is null || !dataSources.Any())
            throw new ArgumentException("Template must have at least one data source.", nameof(dataSources));

        return new ReportTemplate(id, name, branding, defaultFormat, dataSources, schedule);
    }

    /// <summary>
    /// Updates the core properties of the report template.
    /// </summary>
    public void Update(string name, Branding branding, ReportFormat defaultFormat)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name cannot be null or whitespace.", nameof(name));

        Name = name;
        Branding = branding ?? throw new ArgumentNullException(nameof(branding));
        DefaultFormat = defaultFormat;
    }

    /// <summary>
    /// Updates the schedule for the report template.
    /// </summary>
    public void UpdateSchedule(Schedule? newSchedule)
    {
        Schedule = newSchedule;
    }

    /// <summary>
    /// Adds a new data source to the template.
    /// </summary>
    public void AddDataSource(DataSource source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (_dataSources.Any(ds => ds.Name.Equals(source.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A data source with the name '{source.Name}' already exists.");
        }
        _dataSources.Add(source);
    }

    /// <summary>
    /// Removes a data source from the template by its name.
    /// </summary>
    public void RemoveDataSource(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName)) return;
        
        var sourceToRemove = _dataSources.FirstOrDefault(ds => ds.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase));
        if (sourceToRemove != null)
        {
            _dataSources.Remove(sourceToRemove);
        }
    }
}