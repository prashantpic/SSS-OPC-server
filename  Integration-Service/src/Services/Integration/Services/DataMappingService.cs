namespace IntegrationService.Services
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationService.Interfaces;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    // A simple representation of a mapping rule.
    // In a real system, this would be more complex, potentially using JSONPath, JQ, or a custom DSL.
    public class MappingRule
    {
        public string RuleId { get; set; } = string.Empty;
        public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>(); // SourceField -> TargetField
        public List<TransformationStep> Transformations { get; set; } = new List<TransformationStep>();
    }

    public class TransformationStep
    {
        public string TargetField { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., "Concat", "ToUpperCase", "DateTimeFormat"
        public List<string> SourceFields { get; set; } = new List<string>();
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    public class DataMappingService : IDataMapper
    {
        private readonly ILogger<DataMappingService> _logger;
        private readonly IHostEnvironment _environment;
        private readonly ConcurrentDictionary<string, MappingRule> _cachedRules = new ConcurrentDictionary<string, MappingRule>();
        private readonly string _mappingsDirectory;

        public DataMappingService(ILogger<DataMappingService> logger, IHostEnvironment environment)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _mappingsDirectory = Path.Combine(_environment.ContentRootPath, "Mappings"); // e.g., "Mappings/IoT/", "Mappings/DigitalTwin/"
        }

        private async Task<MappingRule?> LoadRuleAsync(string mappingRuleIdOrPath, CancellationToken cancellationToken)
        {
            if (_cachedRules.TryGetValue(mappingRuleIdOrPath, out var cachedRule))
            {
                _logger.LogDebug("Using cached mapping rule for ID/Path: {MappingRuleId}", mappingRuleIdOrPath);
                return cachedRule;
            }

            string filePath = mappingRuleIdOrPath;
            if (!Path.IsPathRooted(filePath))
            {
                filePath = Path.Combine(_mappingsDirectory, mappingRuleIdOrPath); // Assuming ruleId can be a relative path
            }

            if (!File.Exists(filePath))
            {
                // Try with .json extension if not provided
                if (!filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    filePath += ".json";
                }

                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Mapping rule file not found: {FilePath}", filePath);
                    return null;
                }
            }

            try
            {
                _logger.LogInformation("Loading mapping rule from: {FilePath}", filePath);
                var ruleJson = await File.ReadAllTextAsync(filePath, cancellationToken);
                var rule = JsonSerializer.Deserialize<MappingRule>(ruleJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rule != null)
                {
                    _cachedRules.TryAdd(mappingRuleIdOrPath, rule);
                    return rule;
                }
                _logger.LogWarning("Failed to deserialize mapping rule from: {FilePath}", filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading mapping rule from: {FilePath}", filePath);
                return null;
            }
        }

        public async Task<TExternal?> MapToExternalFormatAsync<TInternal, TExternal>(
            TInternal internalData,
            string mappingRuleId,
            CancellationToken cancellationToken)
        {
            var rule = await LoadRuleAsync(mappingRuleId, cancellationToken);
            if (rule == null || internalData == null)
            {
                _logger.LogWarning("Cannot map to external format. Rule '{MappingRuleId}' not loaded or internal data is null.", mappingRuleId);
                if (typeof(TExternal) == typeof(TInternal)) return (TExternal)(object)internalData; // Passthrough if same type and no rule
                return default;
            }

            _logger.LogDebug("Mapping internal data to external format using rule: {RuleId}", rule.RuleId);

            // Simplified mapping logic:
            // This example assumes TInternal and TExternal are dictionaries or can be treated as such via serialization.
            // A real implementation would be much more sophisticated.
            try
            {
                var sourceJson = JsonSerializer.SerializeToElement(internalData);
                var targetData = new Dictionary<string, object?>();

                foreach (var mapping in rule.FieldMappings)
                {
                    if (sourceJson.TryGetProperty(mapping.Key, out var sourceProp))
                    {
                        targetData[mapping.Value] = sourceProp.Clone(); // Clone to avoid issues with JsonElement lifetime
                    }
                }
                
                // Apply transformations (very basic placeholder)
                foreach (var transform in rule.Transformations)
                {
                    if (transform.Type == "ToUpperCase" && transform.SourceFields.Count == 1 && targetData.ContainsKey(transform.SourceFields[0]))
                    {
                        var val = targetData[transform.SourceFields[0]]?.ToString();
                        targetData[transform.TargetField] = val?.ToUpperInvariant();
                    }
                    // Add more transformation logic here
                }

                var mappedJson = JsonSerializer.Serialize(targetData);
                return JsonSerializer.Deserialize<TExternal>(mappedJson);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON serialization/deserialization error during mapping for rule '{RuleId}'.", mappingRuleId);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic error during mapping for rule '{RuleId}'.", mappingRuleId);
                return default;
            }
        }


        public async Task<TInternal?> MapToInternalFormatAsync<TExternal, TInternal>(
            TExternal externalData,
            string mappingRuleId,
            CancellationToken cancellationToken)
        {
            var rule = await LoadRuleAsync(mappingRuleId, cancellationToken);
            if (rule == null || externalData == null)
            {
                 _logger.LogWarning("Cannot map to internal format. Rule '{MappingRuleId}' not loaded or external data is null.", mappingRuleId);
                if (typeof(TInternal) == typeof(TExternal)) return (TInternal)(object)externalData; // Passthrough
                return default;
            }

            _logger.LogDebug("Mapping external data to internal format using rule: {RuleId}", rule.RuleId);
            // Simplified inverse mapping logic (assuming FieldMappings can be inverted)
             try
            {
                var sourceJson = JsonSerializer.SerializeToElement(externalData);
                var targetData = new Dictionary<string, object?>();

                // Inverse field mapping
                foreach (var mapping in rule.FieldMappings) // Key is source (internal), Value is target (external)
                {
                    // So, for internal format, mapping.Value is the source (external field), mapping.Key is target (internal field)
                    if (sourceJson.TryGetProperty(mapping.Value, out var sourceProp))
                    {
                        targetData[mapping.Key] = sourceProp.Clone();
                    }
                }
                
                // Inverse transformations would be more complex and are not shown here.

                var mappedJson = JsonSerializer.Serialize(targetData);
                return JsonSerializer.Deserialize<TInternal>(mappedJson);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON serialization/deserialization error during inverse mapping for rule '{RuleId}'.", mappingRuleId);
                return default;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Generic error during inverse mapping for rule '{RuleId}'.", mappingRuleId);
                return default;
            }
        }
    }
}