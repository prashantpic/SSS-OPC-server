using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SSS.Data.Access;
using System.IO;

namespace SSS.Data.Migrations;

/// <summary>
/// This factory is used by the .NET Entity Framework Core tools (e.g., `dotnet ef migrations add`)
/// to create a DbContext instance at design time. It is required when the DbContext is in a separate
/// project from the startup project, or when the startup project's configuration is not suitable for design-time tasks.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    /// <summary>
    /// Creates a new instance of the ApplicationDbContext for design-time operations.
    /// </summary>
    /// <param name="args">Command-line arguments, not used in this implementation.</param>
    /// <returns>A new ApplicationDbContext instance configured for design-time tools.</returns>
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings.json to get the connection string.
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Get the connection string from the configuration file.
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Create DbContextOptionsBuilder and configure it to use PostgreSQL.
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString,
            // Specify that migrations are located in this assembly.
            opts => opts.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.FullName));

        // Return a new instance of the DbContext.
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}