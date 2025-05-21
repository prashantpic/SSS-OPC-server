using ManagementService.Application.Features.ConfigurationMigrations.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ManagementService.Domain.SeedWork; // For DomainException

namespace ManagementService.Infrastructure.ConfigurationMigration.Parsers;

public class XmlConfigurationParser : IConfigurationFileParser
{
    private readonly ILogger<XmlConfigurationParser> _logger;

    public XmlConfigurationParser(ILogger<XmlConfigurationParser> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<ParsedConfigurationItem>> ParseAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting XML parsing.");
        var parsedItems = new List<ParsedConfigurationItem>();

        try
        {
            XDocument doc = await XDocument.LoadAsync(fileStream, LoadOptions.None, cancellationToken);

            if (doc.Root == null)
            {
                throw new DomainException("XML document is empty or has no root element.");
            }

            // Assuming structure:
            // <Configurations>
            //   <ClientInstance name="Client1">
            //     <Configuration name="ConfigA">
            //       <Version number="1" createdAt="2023-01-01T00:00:00Z">
            //         <Content><![CDATA[...json...]]></Content>
            //       </Version>
            //     </Configuration>
            //   </ClientInstance>
            // </Configurations>
            foreach (var clientElement in doc.Root.Elements("ClientInstance"))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string clientName = clientElement.Attribute("name")?.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(clientName))
                {
                    _logger.LogWarning("Skipping ClientInstance element with missing or empty 'name' attribute.");
                    continue;
                }

                foreach (var configElement in clientElement.Elements("Configuration"))
                {
                    string configName = configElement.Attribute("name")?.Value ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(configName))
                    {
                        _logger.LogWarning("Skipping Configuration element for client '{ClientName}' with missing or empty 'name' attribute.", clientName);
                        continue;
                    }

                    foreach (var versionElement in configElement.Elements("Version"))
                    {
                        string content = versionElement.Element("Content")?.Value ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(content))
                        {
                             _logger.LogWarning("Skipping Version element for client '{ClientName}', config '{ConfigName}' with missing or empty 'Content'.", clientName, configName);
                            continue;
                        }

                        int? versionNumber = null;
                        if (int.TryParse(versionElement.Attribute("number")?.Value, out int vn))
                        {
                            versionNumber = vn;
                        }

                        DateTimeOffset? createdAt = null;
                        if (DateTimeOffset.TryParse(versionElement.Attribute("createdAt")?.Value, out DateTimeOffset ca))
                        {
                            createdAt = ca;
                        }
                        
                        parsedItems.Add(new ParsedConfigurationItem(
                            ClientInstanceName: clientName,
                            ConfigurationName: configName,
                            VersionContent: content,
                            VersionNumber: versionNumber,
                            CreatedAt: createdAt
                        ));
                    }
                }
            }
        }
        catch (System.Xml.XmlException ex)
        {
            _logger.LogError(ex, "XML parsing error: {Message}", ex.Message);
            throw new DomainException($"Error parsing XML file: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during XML parsing.");
            throw new DomainException("An unexpected error occurred during XML parsing.", ex);
        }

        _logger.LogInformation("XML parsing completed. Parsed {Count} items.", parsedItems.Count);
        return parsedItems;
    }
}