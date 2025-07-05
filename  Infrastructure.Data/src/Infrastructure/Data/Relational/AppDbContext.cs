using Microsoft.EntityFrameworkCore;
using Opc.System.Infrastructure.Data.Abstractions;
// using Opc.System.Domain.Models; // Assuming domain models are in a referenced project

#region Placeholder Domain Models
// These would be in the Opc.System.Domain project
public class User
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
public class Role { public Guid RoleId { get; set; } public string Name { get; set; } = string.Empty; }
public class Permission { public Guid PermissionId { get; set; } public string Code { get; set; } = string.Empty; }
public class UserRole { public Guid UserId { get; set; } public Guid RoleId { get; set; } }
public class RolePermission { public Guid RoleId { get; set; } public Guid PermissionId { get; set; } }
public class OpcServer { public Guid ServerId { get; set; } }
public class OpcTag { public Guid TagId { get; set; } public Guid ServerId { get; set; } public string NodeId { get; set; } = string.Empty; }
public class Dashboard { public Guid DashboardId { get; set; } }
// BlockchainTransaction is already defined in IBlockchainLogRepository.cs for this example
#endregion

namespace Opc.System.Infrastructure.Data.Relational
{
    /// <summary>
    /// Represents a session with the PostgreSQL database, allowing for querying and saving of relational data entities using Entity Framework Core.
    /// It also acts as the Unit of Work for relational persistence.
    /// </summary>
    public class AppDbContext : DbContext, IUnitOfWork
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<OpcServer> OpcServers { get; set; }
        public DbSet<OpcTag> OpcTags { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<BlockchainTransaction> BlockchainTransactions { get; set; }

        /// <summary>
        /// Persists all changes made in the current transaction to the database.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The number of state entries written to the database.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Role Entity Configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Permission Entity Configuration
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.PermissionId);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // OpcServer Entity Configuration
            modelBuilder.Entity<OpcServer>().HasKey(e => e.ServerId);

            // OpcTag Entity Configuration
            modelBuilder.Entity<OpcTag>(entity =>
            {
                entity.HasKey(e => e.TagId);
                entity.HasIndex(e => new { e.ServerId, e.NodeId }).IsUnique();
                entity.HasOne<OpcServer>().WithMany().HasForeignKey(e => e.ServerId);
            });
            
            // Dashboard Entity Configuration
            modelBuilder.Entity<Dashboard>().HasKey(e => e.DashboardId);

            // BlockchainTransaction Entity Configuration
            modelBuilder.Entity<BlockchainTransaction>(entity =>
            {
                entity.HasKey(e => e.TransactionId);
                entity.HasIndex(e => e.DataHash).IsUnique();
            });

            // UserRole (Many-to-Many) Configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
                entity.HasOne<Role>().WithMany().HasForeignKey(e => e.RoleId);
            });

            // RolePermission (Many-to-Many) Configuration
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.PermissionId });
                entity.HasOne<Role>().WithMany().HasForeignKey(e => e.RoleId);
                entity.HasOne<Permission>().WithMany().HasForeignKey(e => e.PermissionId);
            });

            // Encryption at Rest Demonstration (REQ-DLP-008)
            // This demonstrates how a value converter would be applied for application-level encryption.
            // The IEncryptionService and EncryptionValueConverter would be defined in a shared/core project.
            /*
            public class ConfigurationSecret { public int Id { get; set; } public string Value { get; set; } }
            public class EncryptionValueConverter : ValueConverter<string, string>
            {
                public EncryptionValueConverter(IEncryptionService encryptionService, ConverterMappingHints mappingHints = null)
                    : base(v => encryptionService.Encrypt(v), v => encryptionService.Decrypt(v), mappingHints)
                { }
            }
            
            // Assume IEncryptionService is available via DI or another mechanism
            // var encryptionService = ...;
            // var encryptionConverter = new EncryptionValueConverter(encryptionService);
            //
            // modelBuilder.Entity<ConfigurationSecret>()
            //     .Property(s => s.Value)
            //     .HasConversion(encryptionConverter);
            */
        }
    }
}