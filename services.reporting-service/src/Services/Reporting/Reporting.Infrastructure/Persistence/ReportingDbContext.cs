using Microsoft.EntityFrameworkCore;
using Reporting.Domain.Aggregates;

namespace Reporting.Infrastructure.Persistence;

/// <summary>
/// Represents a session with the database, allowing querying and saving of entities.
/// It is the central point for EF Core configuration for the Reporting Service.
/// </summary>
public class ReportingDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportingDbContext"/> class.
    /// </summary>
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet for ReportTemplate aggregates.
    /// </summary>
    public DbSet<ReportTemplate> ReportTemplates { get; set; }

    /// <summary>
    /// Gets or sets the DbSet for GeneratedReport aggregates.
    /// </summary>
    public DbSet<GeneratedReport> GeneratedReports { get; set; }

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Scans the assembly for all classes that implement IEntityTypeConfiguration
        // and applies their configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
    }
}