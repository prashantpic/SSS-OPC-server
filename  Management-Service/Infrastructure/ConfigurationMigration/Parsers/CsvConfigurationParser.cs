using ManagementService.Application.Features.ConfigurationMigrations.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using ManagementService.Domain.SeedWork; // For DomainException

namespace ManagementService.Infrastructure.ConfigurationMigration.Parsers;

public class CsvConfigurationParser : IConfigurationFileParser
{
    private readonly ILogger<CsvConfigurationParser> _logger;

    public CsvConfigurationParser(ILogger<CsvConfigurationParser> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<ParsedConfigurationItem>> ParseAsync(Stream fileStream, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting CSV parsing.");
        var parsedItems = new List<ParsedConfigurationItem>();

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true, // Assume header exists
            MissingFieldFound = null, // Allow missing fields by not throwing
            HeaderValidated = null, // Allow headers to not match perfectly if needed
            TrimOptions = TrimOptions.Trim,
        };

        using (var reader = new StreamReader(fileStream, leaveOpen: true)) // leaveOpen for external stream management
        using (var csv = new CsvReader(reader, csvConfig))
        {
            // Optional: Register a ClassMap if CSV headers don't match ParsedConfigurationItem properties
            // csv.Context.RegisterClassMap<MyCsvClassMap>();
            
            try
            {
                await foreach (var record in csv.GetRecordsAsync<CsvRowModel>().WithCancellation(cancellationToken))
                {
                    // Perform transformations or validations if needed
                    if (string.IsNullOrWhiteSpace(record.ClientInstanceName) || string.IsNullOrWhiteSpace(record.ConfigurationName) || string.IsNullOrWhiteSpace(record.VersionContent))
                    {
                        _logger.LogWarning("Skipping CSV row due to missing required fields: Client='{Client}', Config='{Config}'", record.ClientInstanceName, record.ConfigurationName);
                        continue;
                    }

                    parsedItems.Add(new ParsedConfigurationItem(
                        ClientInstanceName: record.ClientInstanceName,
                        ConfigurationName: record.ConfigurationName,
                        VersionContent: record.VersionContent,
                        VersionNumber: record.VersionNumber,
                        CreatedAt: record.CreatedAt
                    ));
                }
            }
            catch (CsvHelperException ex)
            {
                _logger.LogError(ex, "CSV parsing error at row {RowNumber}: {Message}", csv.Parser.Row, ex.Message);
                throw new DomainException($"Error parsing CSV file at row {csv.Parser.Row}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CSV parsing.");
                throw new DomainException("An unexpected error occurred during CSV parsing.", ex);
            }
        }
        _logger.LogInformation("CSV parsing completed. Parsed {Count} items.", parsedItems.Count);
        return parsedItems;
    }

    // Internal model to map CSV rows
    private class CsvRowModel
    {
        public string ClientInstanceName { get; set; } = string.Empty;
        public string ConfigurationName { get; set; } = string.Empty;
        public string VersionContent { get; set; } = string.Empty;
        public int? VersionNumber { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        // Add other columns as expected in the CSV
    }
}