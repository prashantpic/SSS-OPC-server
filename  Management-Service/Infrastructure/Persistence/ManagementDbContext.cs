using ManagementService.Application.Abstractions.Data;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate;
using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;
using ManagementService.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using MediatR;

namespace ManagementService.Infrastructure.Persistence;

public class ManagementDbContext : DbContext
{
    private readonly IMediator? _mediator; // Optional: for domain event dispatching directly from DbContext

    public DbSet<ClientInstance> ClientInstances { get; set; } = null!;
    public DbSet<ClientConfiguration> ClientConfigurations { get; set; } = null!;
    // ConfigurationVersion is an owned entity type, no DbSet needed.
    public DbSet<BulkOperationJob> BulkOperationJobs { get; set; } = null!;
    public DbSet<ConfigurationMigrationJob> ConfigurationMigrationJobs { get; set; } = null!;


    public ManagementDbContext(DbContextOptions<ManagementDbContext> options, IMediator? mediator = null) : base(options)
    {
        _mediator = mediator;
    }
     public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
    {
        // Constructor for DI scenarios where IMediator is not injected directly here, but via UnitOfWork
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); // Scans for IEntityTypeConfiguration

        // Explicit configuration for ClientInstance and its Value Object
        modelBuilder.Entity<ClientInstance>(cfg =>
        {
            cfg.ToTable("ClientInstances");
            cfg.HasKey(ci => ci.Id);
            cfg.Property(ci => ci.Name).IsRequired().HasMaxLength(255);

            cfg.OwnsOne(ci => ci.Status, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.Property(s => s.Value)
                    .HasColumnName("StatusValue") // Avoid conflict with property name 'Status'
                    .IsRequired()
                    .HasMaxLength(50);
            });
            
            cfg.HasOne(ci => ci.ClientConfiguration)
               .WithOne() // No navigation property back from ClientConfiguration explicitly defined here
               .HasForeignKey<ClientInstance>(ci => ci.ClientConfigurationId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

            cfg.Ignore(e => e.DomainEvents);
        });

        // Explicit configuration for ClientConfiguration and its owned ConfigurationVersion
        modelBuilder.Entity<ClientConfiguration>(cfg =>
        {
            cfg.ToTable("ClientConfigurations");
            cfg.HasKey(cc => cc.Id);
            cfg.Property(cc => cc.Name).IsRequired().HasMaxLength(255);
            cfg.Property(cc => cc.ClientInstanceId).IsRequired();
            
            // Owned collection of ConfigurationVersion
            cfg.OwnsMany(cc => cc.Versions, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("ConfigurationVersions");
                ownedNavigationBuilder.WithOwner().HasForeignKey("ClientConfigurationId"); // Shadow FK
                ownedNavigationBuilder.HasKey(cv => cv.Id); // Each version has its own ID
                ownedNavigationBuilder.Property(cv => cv.VersionNumber).IsRequired();
                ownedNavigationBuilder.Property(cv => cv.Content).IsRequired(); // Consider max length or nvarchar(max)
                ownedNavigationBuilder.Property(cv => cv.CreatedAt).IsRequired();
                ownedNavigationBuilder.Ignore(cv => cv.DomainEvents);
            });

            cfg.Navigation(cc => cc.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
            cfg.Ignore(e => e.DomainEvents);
        });

        // Explicit configuration for BulkOperationJob and its Value Objects/Owned Entities
        modelBuilder.Entity<BulkOperationJob>(cfg =>
        {
            cfg.ToTable("BulkOperationJobs");
            cfg.HasKey(j => j.Id);

            cfg.OwnsOne(j => j.OperationType, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.Property(ot => ot.Value)
                    .HasColumnName("OperationTypeValue")
                    .IsRequired()
                    .HasMaxLength(100);
            });

            cfg.OwnsOne(j => j.Status, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.Property(s => s.Value)
                    .HasColumnName("StatusValue")
                    .IsRequired()
                    .HasMaxLength(50);
            });

            cfg.Property(j => j.ParametersJson).HasColumnType("nvarchar(max)");
            cfg.Property(j => j.ResultDetails).HasMaxLength(1000);

            cfg.OwnsMany(j => j.ProgressDetails, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToTable("BulkOperationTaskDetails");
                ownedNavigationBuilder.WithOwner().HasForeignKey("BulkOperationJobId");
                ownedNavigationBuilder.HasKey(pd => pd.Id);
                ownedNavigationBuilder.Property(pd => pd.ClientInstanceId).IsRequired();
                ownedNavigationBuilder.Property(pd => pd.Status).IsRequired().HasMaxLength(50);
                ownedNavigationBuilder.Property(pd => pd.Details).HasMaxLength(500);
                ownedNavigationBuilder.Ignore(pd => pd.DomainEvents);
            });
            cfg.Navigation(j => j.ProgressDetails).UsePropertyAccessMode(PropertyAccessMode.Field);
            cfg.Ignore(e => e.DomainEvents);
        });

        // Explicit configuration for ConfigurationMigrationJob
        modelBuilder.Entity<ConfigurationMigrationJob>(cfg =>
        {
            cfg.ToTable("ConfigurationMigrationJobs");
            cfg.HasKey(j => j.Id);
            cfg.Property(j => j.FileName).IsRequired().HasMaxLength(255);
            cfg.Property(j => j.SourceFormat).IsRequired().HasMaxLength(50);
            
            cfg.OwnsOne(j => j.Status, ownedNavigationBuilder => // Reusing JobStatus VO
            {
                ownedNavigationBuilder.Property(s => s.Value)
                    .HasColumnName("StatusValue")
                    .IsRequired()
                    .HasMaxLength(50);
            });

            cfg.Property(j => j.ResultDetails).HasMaxLength(1000);
            
            // Storing list of strings for ValidationMessages. EF Core can map this to a related table.
            // This requires careful configuration, often using a wrapper entity or JSON serialization for simplicity.
            // For a separate table for strings:
            cfg.Property(j => j.ValidationMessages)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("nvarchar(max)"); // Store as JSON string for simplicity

            cfg.Navigation(j => j.ValidationMessages).UsePropertyAccessMode(PropertyAccessMode.Field);
            cfg.Ignore(e => e.DomainEvents);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Domain event dispatching can be handled by UnitOfWork or here if IMediator is injected.
        // If UnitOfWork handles it, this becomes a standard SaveChangesAsync.
        // For this example, assume UnitOfWork wrapper will call this and then dispatch events.
        return await base.SaveChangesAsync(cancellationToken);
    }
}