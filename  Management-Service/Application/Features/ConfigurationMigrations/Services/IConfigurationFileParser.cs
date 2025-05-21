using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Application.Features.ConfigurationMigrations.Services;

// Interface defining the contract for configuration file parsers (e.g., CSV, XML).
public interface IConfigurationFileParser
{
    /// <summary>
    /// Parses a file stream into a list of intermediate configuration items.
    /// </summary>
    /// <param name="fileStream">The stream containing the file content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of parsed configuration items.</returns>
    Task<IEnumerable<ParsedConfigurationItem>> ParseAsync(Stream fileStream, CancellationToken cancellationToken);
}

// Intermediate structure for parsed configuration data
// This definition was previously in ConfigurationMigrationOrchestrator.cs.
// Moving it here makes it more accessible for parser implementations.
public record ParsedConfigurationItem(
    string ClientInstanceName, // Or ID, depending on source file format
    string ConfigurationName,
    string VersionContent, // Content of this specific version
    int? VersionNumber = null, // Optional, if source file specifies it
    System.DateTimeOffset? CreatedAt = null // Optional, default to migration time if null
);