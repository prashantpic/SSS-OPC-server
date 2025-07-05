using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Opc.System.Services.Authentication.Domain.Entities;

namespace Opc.System.Services.Authentication.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context that represents the session with the database,
/// allowing identity and audit entities to be queried and saved.
/// </summary>
public class AuthDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    /// <summary>
    /// DbSet for security audit log entries.
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; }

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure AuditLog entity
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Details).HasColumnType("jsonb"); // For PostgreSQL
            entity.HasIndex(e => new { e.Timestamp, e.EventType });
        });

        // Seed initial data
        SeedRoles(builder);
    }
    
    private void SeedRoles(ModelBuilder builder)
    {
        var adminRoleId = new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1");
        var userRoleId = new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2");

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "System Administrator with full access.",
                IsSystemRole = true
            },
            new ApplicationRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standard user with basic permissions.",
                IsSystemRole = true
            }
        );
    }
}