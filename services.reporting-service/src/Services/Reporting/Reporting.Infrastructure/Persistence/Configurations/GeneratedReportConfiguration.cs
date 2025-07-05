using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reporting.Domain.Aggregates;

namespace Reporting.Infrastructure.Persistence.Configurations;

public class GeneratedReportConfiguration : IEntityTypeConfiguration<GeneratedReport>
{
    public void Configure(EntityTypeBuilder<GeneratedReport> builder)
    {
        builder.ToTable("GeneratedReports");

        builder.HasKey(gr => gr.Id);

        builder.Property(gr => gr.ReportTemplateId)
            .IsRequired();

        builder.HasIndex(gr => gr.ReportTemplateId);

        builder.Property(gr => gr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(gr => gr.Format)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);
        
        builder.Property(gr => gr.FilePath)
            .HasMaxLength(1024);
            
        builder.Property(gr => gr.FailureReason);
    }
}