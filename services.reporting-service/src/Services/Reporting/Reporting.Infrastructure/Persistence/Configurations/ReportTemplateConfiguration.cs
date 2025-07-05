using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Reporting.Domain.Aggregates;
using Reporting.Domain.ValueObjects;

namespace Reporting.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity type configuration for the ReportTemplate aggregate root.
/// </summary>
public class ReportTemplateConfiguration : IEntityTypeConfiguration<ReportTemplate>
{
    public void Configure(EntityTypeBuilder<ReportTemplate> builder)
    {
        builder.ToTable("ReportTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.DefaultFormat)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        // Configure owned types for value objects. They will be mapped to columns
        // in the same ReportTemplates table.
        builder.OwnsOne(t => t.Branding, brandingBuilder =>
        {
            brandingBuilder.Property(b => b.LogoUrl).HasMaxLength(512).HasColumnName("Branding_LogoUrl");
            brandingBuilder.Property(b => b.PrimaryColor).HasMaxLength(20).HasColumnName("Branding_PrimaryColor");
            brandingBuilder.Property(b => b.CompanyName).HasMaxLength(100).HasColumnName("Branding_CompanyName");
        });

        builder.OwnsOne(t => t.Schedule, scheduleBuilder =>
        {
            scheduleBuilder.Property(s => s.CronExpression).HasMaxLength(50).HasColumnName("Schedule_CronExpression");
        });

        // Configure the DataSources collection to be stored as a JSON string in a single column.
        // This is a simple approach for complex, non-relational data within an entity.
        var jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
        };
        
        builder.Property(t => t.DataSources)
            .HasConversion(
                v => JsonConvert.SerializeObject(v, jsonSettings),
                v => JsonConvert.DeserializeObject<List<DataSource>>(v, jsonSettings) ?? new List<DataSource>()
            )
            .HasColumnType("jsonb"); // Use jsonb for PostgreSQL for better performance and indexing
    }
}