using Microsoft.EntityFrameworkCore;
using Opc.System.Services.Management.Domain.Aggregates;

namespace Opc.System.Services.Management.Infrastructure.Persistence
{
    /// <summary>
    /// The EF Core database context for the Management microservice,
    /// responsible for all interactions with the relational database.
    /// </summary>
    public class ManagementDbContext : DbContext
    {
        public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
        {
        }

        public DbSet<OpcClientInstance> OpcClientInstances { get; set; }
        public DbSet<MigrationStrategy> MigrationStrategies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure OpcClientInstance Aggregate
            modelBuilder.Entity<OpcClientInstance>(builder =>
            {
                builder.ToTable("OpcClientInstances");

                builder.HasKey(ci => ci.Id);

                builder.Property(ci => ci.Id)
                    .HasConversion(
                        clientInstanceId => clientInstanceId.Value,
                        value => new ClientInstanceId(value));

                builder.Property(ci => ci.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // Store the complex ClientConfiguration object as a single JSONB column
                builder.OwnsOne(ci => ci.Configuration, ownedNavigationBuilder =>
                {
                    ownedNavigationBuilder.ToJson();
                    ownedNavigationBuilder.OwnsMany(c => c.ServerConnections);
                    ownedNavigationBuilder.OwnsMany(c => c.TagConfigurations);
                });

                builder.Property(ci => ci.HealthStatus)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(20);

                builder.Property(ci => ci.IsActive)
                    .IsRequired();
            });

            // Configure MigrationStrategy Aggregate
            modelBuilder.Entity<MigrationStrategy>(builder =>
            {
                builder.ToTable("MigrationStrategies");
                
                builder.HasKey(ms => ms.Id);

                builder.Property(ms => ms.Id)
                    .HasConversion(
                        migrationStrategyId => migrationStrategyId.Value,
                        value => new MigrationStrategyId(value));

                builder.Property(ms => ms.SourceSystem)
                    .IsRequired()
                    .HasMaxLength(50);
                
                builder.Property(ms => ms.MappingRules).HasColumnType("jsonb");
            });
        }
    }
}