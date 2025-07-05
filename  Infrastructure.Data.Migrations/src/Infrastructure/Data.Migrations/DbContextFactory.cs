using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Opc.System.Infrastructure.Data; // Reference to the project with the DbContext

namespace Opc.System.Infrastructure.Data.Migrations;

/// <summary>
/// Factory for creating ApplicationDbContext instances at design time.
/// Used by EF Core tools to generate and apply migrations from this separate project.
/// </summary>
public class DbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Creates a new instance of the ApplicationDbContext.
    /// This method is called by the EF Core tools (e.g., `dotnet ef migrations add`) to get a
    /// configured DbContext instance for scaffolding migrations.
    /// </summary>
    /// <param name="args">Command-line arguments (not used by this implementation).</param>
    /// <returns>A configured ApplicationDbContext instance.</returns>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration to read the connection string from appsettings.Development.json
        // This file should NOT be checked into source control if it contains secrets.
        // For team environments, consider using user secrets (`dotnet user-secrets init`)
        // and AddUserSecrets() for a more secure approach to handling development connection strings.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DesignTimeConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Could not find a connection string named 'DesignTimeConnection'.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Configure the DbContext to use PostgreSQL.
        optionsBuilder.UseNpgsql(connectionString, b =>
        {
            // Specify that the migrations assembly is this current project.
            // This is the crucial setting that tells EF Core to look for migration files
            // in this project (`Opc.System.Infrastructure.Data.Migrations`) rather than the
            // project where the DbContext is defined.
            b.MigrationsAssembly(typeof(DbContextFactory).Assembly.FullName);
        });

        // The ApplicationDbContext constructor is expected to accept DbContextOptions<ApplicationDbContext>.
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}