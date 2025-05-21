using ManagementService.Application.Features.ConfigurationMigrations.Services;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using System.Collections.Generic;
using System.Linq;

namespace ManagementService.Infrastructure.ConfigurationMigration.Transformers;

public class DefaultConfigurationTransformer
{
    private readonly ILogger<DefaultConfigurationTransformer> _logger;

    public DefaultConfigurationTransformer(ILogger<DefaultConfigurationTransformer> logger)
    {
        _logger = logger;
    }

    // Transforms parsed items into a list of MigratedClientConfiguration objects.
    // These are intermediate representations, not yet full domain entities.
    public List<MigratedClientConfiguration> Transform(IEnumerable<ParsedConfigurationItem> parsedItems)
    {
        _logger.LogInformation("Transforming {Count} parsed items.", parsedItems.Count());
        var migratedConfigs = new List<MigratedClientConfiguration>();

        var groupedByClientAndConfigName = parsedItems
            .GroupBy(item => new { item.ClientInstanceName, item.ConfigurationName });

        foreach (var group in groupedByClientAndConfigName)
        {
            var clientName = group.Key.ClientInstanceName;
            var configName = group.Key.ConfigurationName;

            var versions = group
                .Select(item => new MigratedConfigurationVersion(
                    Content: item.VersionContent,
                    VersionNumber: item.VersionNumber,
                    CreatedAt: item.CreatedAt
                ))
                .OrderBy(v => v.VersionNumber ?? int.MaxValue) // Ensure versions are ordered
                .ThenBy(v => v.CreatedAt ?? DateTimeOffset.MaxValue)
                .ToList();
            
            // Assign sequential version numbers if not provided, or ensure uniqueness
            for(int i = 0; i < versions.Count; i++)
            {
                if (!versions[i].VersionNumber.HasValue)
                {
                    versions[i].VersionNumber = i + 1;
                }
                 if (!versions[i].CreatedAt.HasValue)
                {
                    versions[i].CreatedAt = DateTimeOffset.UtcNow; // Default to now if not provided
                }
            }


            migratedConfigs.Add(MigratedClientConfiguration.Create(clientName, configName, versions));
        }
        _logger.LogInformation("Transformation complete. Produced {Count} migrated configuration structures.", migratedConfigs.Count);
        return migratedConfigs;
    }
}

// These helper classes are defined here as per SDS.
// They represent the structure used by the Transformer, Validator, and Saver in the migration pipeline.

public class MigratedClientConfiguration
{
    public Guid Id { get; } // Unique ID for this migration representation
    public string ClientInstanceName { get; }
    public Guid? ClientInstanceId { get; set; } // To be resolved by validator/saver
    public string ConfigurationName { get; }
    public List<MigratedConfigurationVersion> Versions { get; }
    public List<ValidationError> ValidationErrors { get; } = new List<ValidationError>();


    private MigratedClientConfiguration(string clientInstanceName, string configurationName, List<MigratedConfigurationVersion> versions)
    {
        Id = Guid.NewGuid();
        ClientInstanceName = clientInstanceName;
        ConfigurationName = configurationName;
        Versions = versions;
    }
    
    public static MigratedClientConfiguration Create(string clientInstanceName, string configurationName, List<MigratedConfigurationVersion> versions)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(clientInstanceName)) throw new ArgumentNullException(nameof(clientInstanceName));
        if (string.IsNullOrWhiteSpace(configurationName)) throw new ArgumentNullException(nameof(configurationName));
        if (versions == null || !versions.Any()) throw new ArgumentException("Versions list cannot be null or empty.", nameof(versions));

        return new MigratedClientConfiguration(clientInstanceName, configurationName, versions);
    }
}

public class MigratedConfigurationVersion
{
    public string Content { get; }
    public int? VersionNumber { get; set; } // Settable if needs to be adjusted post-parsing
    public DateTimeOffset? CreatedAt { get; set; } // Settable

    public MigratedConfigurationVersion(string content, int? versionNumber, DateTimeOffset? createdAt)
    {
        Content = content;
        VersionNumber = versionNumber;
        CreatedAt = createdAt;
    }
}