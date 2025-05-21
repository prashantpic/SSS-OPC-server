using ManagementService.Domain.SeedWork;
using System;

namespace ManagementService.Domain.Aggregates.ClientInstanceAggregate;

public class ConfigurationVersion : Entity
{
    public int VersionNumber { get; private set; }
    public string Content { get; private set; } // Configuration content (e.g., JSON, XML)
    public DateTimeOffset CreatedAt { get; private set; }

    // Private constructor for EF Core and factory methods
    private ConfigurationVersion() { }

    // Internal factory method, typically called by ClientConfiguration
    internal static ConfigurationVersion Create(int versionNumber, string content, DateTimeOffset createdAt)
    {
        if (versionNumber <= 0)
            throw new DomainException("Version number must be positive.");
        if (string.IsNullOrWhiteSpace(content)) // Content can be empty string but not null/whitespace
            throw new DomainException("Configuration content cannot be null or whitespace.");


        return new ConfigurationVersion
        {
            Id = Guid.NewGuid(),
            VersionNumber = versionNumber,
            Content = content,
            CreatedAt = createdAt
        };
    }

    // Configuration versions are typically immutable once created.
}