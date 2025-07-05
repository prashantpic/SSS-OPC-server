using DataService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DataService.Infrastructure.Persistence.Relational
{
    /// <summary>
    /// Represents the Entity Framework Core session with the relational database (PostgreSQL).
    /// It manages entity-to-table mappings, change tracking, and transaction saving.
    /// This class acts as the implementation of the Unit of Work pattern for relational data.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for UserConfiguration entities.
        /// </summary>
        public DbSet<UserConfiguration> UserConfigurations => Set<UserConfiguration>();

        /// <summary>
        /// DbSet for DataRetentionPolicy entities.
        /// </summary>
        public DbSet<DataRetentionPolicy> DataRetentionPolicies => Set<DataRetentionPolicy>();

        /// <summary>
        /// Configures the database model when it is being created.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all configurations defined in the current assembly.
            // This is a clean way to keep entity configurations in separate files.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Example of fluent API configuration directly in this method:
            modelBuilder.Entity<UserConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.Value).IsRequired();
            });
            
            modelBuilder.Entity<DataRetentionPolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DataType).IsRequired()
                    .HasConversion<string>() // Store enum as string
                    .HasMaxLength(50);
                entity.Property(e => e.Action).IsRequired()
                    .HasConversion<string>() // Store enum as string
                    .HasMaxLength(50);
                entity.Property(e => e.RetentionPeriodDays).IsRequired();
                entity.Property(e => e.ArchiveLocation).HasMaxLength(500);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}