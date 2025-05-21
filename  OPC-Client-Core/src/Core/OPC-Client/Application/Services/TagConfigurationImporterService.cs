using OPC.Client.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;
// To use CsvHelper, ensure the package is referenced in the .csproj
// Example: <PackageReference Include="CsvHelper" Version="30.0.1" />
using CsvHelper;
using CsvHelper.Configuration;

namespace OPC.Client.Core.Application.Services
{
    /// <summary>
    /// Represents configuration for a single tag, typically imported from a file.
    /// </summary>
    public class TagConfiguration // Defined here for simplicity if not part of a broader ClientConfiguration DTO
    {
        public string TagId { get; set; } = string.Empty;
        public NodeAddress NodeAddress { get; set; } = new NodeAddress(string.Empty, null);
        public string DisplayName { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsCriticalWrite { get; set; } = false;
        public bool IsMonitored { get; set; } = false;
        public double? DeadbandValue { get; set; }
        public string? DataValidationRule { get; set; }
    }

    /// <summary>
    /// Application service responsible for importing tag configurations from files (e.g., CSV, XML).
    /// Handles the logic for parsing tag configuration files and transforming them into internal representations.
    /// REQ-CSVC-008
    /// </summary>
    public class TagConfigurationImporterService
    {
        private readonly ILogger<TagConfigurationImporterService> _logger;

        public TagConfigurationImporterService(ILogger<TagConfigurationImporterService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Imports tag configurations from a specified file path.
        /// </summary>
        /// <param name="filePath">The path to the configuration file.</param>
        /// <returns>A list of imported TagConfiguration objects.</returns>
        public async Task<List<TagConfiguration>> ImportConfigurationsAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError("Tag configuration file not found: {FilePath}", filePath);
                throw new FileNotFoundException($"Tag configuration file not found: {filePath}", filePath);
            }

            var fileExtension = Path.GetExtension(filePath)?.ToLowerInvariant();
            var configurations = new List<TagConfiguration>();

            _logger.LogInformation("Attempting to import tag configurations from {FilePath}", filePath);

            try
            {
                switch (fileExtension)
                {
                    case ".csv":
                        configurations = await ParseCsvAsync(filePath);
                        break;
                    case ".xml":
                        configurations = await ParseXmlAsync(filePath);
                        break;
                    default:
                        _logger.LogError("Unsupported tag configuration file type: {FileExtension} for file {FilePath}", fileExtension, filePath);
                        throw new UnsupportedFileTypeException($"Unsupported file type: {fileExtension}");
                }

                _logger.LogInformation("Successfully imported {Count} tag configurations from {FilePath}", configurations.Count, filePath);
                return configurations;
            }
            catch (Exception ex) when (ex is not FileNotFoundException && ex is not UnsupportedFileTypeException)
            {
                _logger.LogError(ex, "Error parsing tag configuration file {FilePath}", filePath);
                throw new FileParsingException($"Error parsing tag configuration file {filePath}", ex);
            }
        }

        private async Task<List<TagConfiguration>> ParseCsvAsync(string filePath)
        {
            var records = new List<TagConfiguration>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null, // Allow missing headers or handle them manually
                MissingFieldFound = null, // Allow missing fields or handle them manually
                PrepareHeaderForMatch = args => args.Header.ToLower(),
            };

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                // It's good practice to define a map for CSV Helper if headers are complex
                // For simplicity, we'll try to read by header name directly or index.
                // csv.Context.RegisterClassMap<TagConfigurationMap>(); // If using a map

                await csv.ReadAsync();
                csv.ReadHeader();
                while (await csv.ReadAsync())
                {
                    try
                    {
                        var tagConfig = new TagConfiguration
                        {
                            TagId = csv.GetField<string>("TagId") ?? Guid.NewGuid().ToString(),
                            NodeAddress = new NodeAddress(
                                csv.GetField<string>("NodeIdentifier") ?? string.Empty,
                                csv.GetField<ushort?>("NamespaceIndex")
                            ),
                            DisplayName = csv.GetField<string>("DisplayName") ?? string.Empty,
                            DataType = csv.GetField<string>("DataType") ?? string.Empty,
                            IsCriticalWrite = csv.GetField<bool?>("IsCriticalWrite") ?? false,
                            IsMonitored = csv.GetField<bool?>("IsMonitored") ?? false,
                            DeadbandValue = csv.GetField<double?>("DeadbandValue"),
                            DataValidationRule = csv.GetField<string>("DataValidationRule")
                        };

                        if (string.IsNullOrWhiteSpace(tagConfig.NodeAddress.Identifier))
                        {
                            _logger.LogWarning("Skipping CSV row at line {Line} due to missing NodeIdentifier in file {FilePath}", csv.Context.Parser.Row, filePath);
                            continue;
                        }
                        records.Add(tagConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing CSV row at line {Line} in file {FilePath}", csv.Context.Parser.Row, filePath);
                    }
                }
            }
            return records;
        }

        private async Task<List<TagConfiguration>> ParseXmlAsync(string filePath)
        {
            var records = new List<TagConfiguration>();
            var xmlDoc = new XmlDocument();
            await Task.Run(() => xmlDoc.Load(filePath)); // Load synchronously on a background thread

            var tagNodes = xmlDoc.SelectNodes("//Tags/Tag"); // Assuming a structure like <Tags><Tag>...</Tag></Tags>

            if (tagNodes != null)
            {
                foreach (XmlNode tagNode in tagNodes)
                {
                    try
                    {
                        var tagConfig = new TagConfiguration
                        {
                            TagId = tagNode.SelectSingleNode("TagId")?.InnerText ?? Guid.NewGuid().ToString(),
                            NodeAddress = new NodeAddress(
                                tagNode.SelectSingleNode("NodeAddress/Identifier")?.InnerText ?? string.Empty,
                                ushort.TryParse(tagNode.SelectSingleNode("NodeAddress/NamespaceIndex")?.InnerText, out ushort nsIdx) ? nsIdx : (ushort?)null
                            ),
                            DisplayName = tagNode.SelectSingleNode("DisplayName")?.InnerText ?? string.Empty,
                            DataType = tagNode.SelectSingleNode("DataType")?.InnerText ?? string.Empty,
                            IsCriticalWrite = bool.TryParse(tagNode.SelectSingleNode("IsCriticalWrite")?.InnerText, out bool critical) && critical,
                            IsMonitored = bool.TryParse(tagNode.SelectSingleNode("IsMonitored")?.InnerText, out bool monitored) && monitored,
                            DeadbandValue = double.TryParse(tagNode.SelectSingleNode("DeadbandValue")?.InnerText, out double dbVal) ? dbVal : (double?)null,
                            DataValidationRule = tagNode.SelectSingleNode("DataValidationRule")?.InnerText
                        };

                        if (string.IsNullOrWhiteSpace(tagConfig.NodeAddress.Identifier))
                        {
                            _logger.LogWarning("Skipping XML tag entry due to missing NodeIdentifier in file {FilePath}", filePath);
                            continue;
                        }
                        records.Add(tagConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing XML tag entry in file {FilePath}", filePath);
                    }
                }
            }
            return records;
        }
    }

    // Custom exceptions for this service
    public class UnsupportedFileTypeException : Exception
    {
        public UnsupportedFileTypeException(string message) : base(message) { }
        public UnsupportedFileTypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class FileParsingException : Exception
    {
        public FileParsingException(string message) : base(message) { }
        public FileParsingException(string message, Exception innerException) : base(message, innerException) { }
    }
}