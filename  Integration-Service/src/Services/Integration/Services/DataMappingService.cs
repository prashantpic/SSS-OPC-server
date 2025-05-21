using IntegrationService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json; // Added for JSON deserialization
using System.Collections.Generic; // Added for IEnumerable
using System.Linq; // Added for Linq operations

namespace IntegrationService.Services
{
    /// <summary>
    /// Implements IDataMapper, loads and applies data transformation rules.
    /// </summary>
    public class DataMappingService : IDataMapper
    {
        private readonly ILogger<DataMappingService> _logger;
        private readonly string _mappingRulePathBase;
        private readonly ConcurrentDictionary<string, JsonDocument> _mappingRulesCache = new ConcurrentDictionary<string, JsonDocument>();

        public DataMappingService(ILogger<DataMappingService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _mappingRulePathBase = configuration["IntegrationSettings:MappingRulePathBase"] ?? "Mappings";
             _logger.LogInformation("DataMappingService initialized with base path {BasePath}", _mappingRulePathBase);
        }

        public TTarget Map<TSource, TTarget>(TSource sourceData, string mappingRuleId) where TTarget : new()
        {
            if (!_mappingRulesCache.TryGetValue(mappingRuleId, out var rulesDoc) || rulesDoc == null)
            {
                 _logger.LogWarning("Mapping rules for '{MappingRuleId}' not loaded. Attempting to load.", mappingRuleId);
                 LoadMappingRulesAsync(mappingRuleId).GetAwaiter().GetResult();
                 if (!_mappingRulesCache.TryGetValue(mappingRuleId, out rulesDoc) || rulesDoc == null)
                 {
                     _logger.LogError("Failed to load mapping rules for '{MappingRuleId}'. Cannot perform mapping.", mappingRuleId);
                     throw new System.InvalidOperationException($"Mapping rules for '{mappingRuleId}' are not loaded.");
                 }
            }

            _logger.LogDebug("Applying mapping rules '{MappingRuleId}' for data of type {SourceType} to {TargetType}", mappingRuleId, typeof(TSource).Name, typeof(TTarget).Name);
            var targetData = new TTarget();

            // --- Placeholder Mapping Logic using JSON rules ---
            // This assumes rulesDoc.RootElement contains an array of mapping definitions.
            // Example rule: { "SourceField": "path.to.source", "TargetField": "path.to.target", "Type": "string" }
            try
            {
                JsonElement mappingInstructions = rulesDoc.RootElement.GetProperty("Rules"); // Assuming rules are under a "Rules" property

                if (mappingInstructions.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement rule in mappingInstructions.EnumerateArray())
                    {
                        string? sourceFieldPath = rule.TryGetProperty("SourceField", out var sfp) ? sfp.GetString() : null;
                        string? targetFieldPath = rule.TryGetProperty("TargetField", out var tfp) ? tfp.GetString() : null;
                        // string fieldType = rule.TryGetProperty("Type", out var typeProp) ? typeProp.GetString() : "object"; // Future use

                        if (sourceFieldPath != null && targetFieldPath != null)
                        {
                            try
                            {
                                object? sourceValue = GetValueFromPath(sourceData, sourceFieldPath);
                                if (sourceValue != null)
                                {
                                    SetValueToPath(targetData, targetFieldPath, sourceValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error applying individual mapping rule: Source '{SourcePath}' to Target '{TargetPath}' for rule ID '{RuleId}'. Skipping this field.", sourceFieldPath, targetFieldPath, mappingRuleId);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Mapping rules for '{MappingRuleId}' are not in the expected array format under 'Rules' property.", mappingRuleId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing mapping rules for '{MappingRuleId}'.", mappingRuleId);
                // Potentially return partially mapped object or throw, depending on requirements
            }

            return targetData;
        }

        // Helper to get value from potentially nested path
        private object? GetValueFromPath(object? obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path)) return null;
            var properties = path.Split('.');
            object? current = obj;
            foreach (var propName in properties)
            {
                if (current == null) return null;
                var propInfo = current.GetType().GetProperty(propName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propInfo == null)
                {
                    // Try dictionary access if it's a JObject or Dictionary
                    if (current is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        if (jsonElement.TryGetProperty(propName, out var subElement))
                        {
                            current = ExtractJsonElementValue(subElement);
                            continue;
                        }
                    }
                    if (current is IDictionary<string, object> dict)
                    {
                        if (dict.TryGetValue(propName, out var val))
                        {
                            current = val;
                            continue;
                        }
                    }
                    _logger.LogTrace("Property '{Property}' not found on path '{Path}' for object of type '{ObjectType}'.", propName, path, current.GetType().Name);
                    return null;
                }
                current = propInfo.GetValue(current);
            }
            return current;
        }

        private object? ExtractJsonElementValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out long lVal) ? lVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.Clone() // Return JsonElement for further processing if complex type
            };
        }

        // Helper to set value to potentially nested path
        private void SetValueToPath(object obj, string path, object? value)
        {
            var properties = path.Split('.');
            object current = obj;
            for (int i = 0; i < properties.Length - 1; i++)
            {
                var propInfo = current.GetType().GetProperty(properties[i], System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propInfo == null)
                {
                    _logger.LogWarning("Intermediate property '{Property}' not found on path '{Path}'. Cannot set value.", properties[i], path);
                    return;
                }
                object? nextObj = propInfo.GetValue(current);
                if (nextObj == null)
                {
                    if (!propInfo.CanWrite) {
                        _logger.LogWarning("Intermediate property '{Property}' on path '{Path}' is null and not writable. Cannot set value.", properties[i], path);
                        return;
                    }
                    // If property type has parameterless constructor, try to instantiate
                    if (propInfo.PropertyType.GetConstructor(Type.EmptyTypes) != null) {
                        nextObj = Activator.CreateInstance(propInfo.PropertyType);
                        propInfo.SetValue(current, nextObj);
                    } else {
                         _logger.LogWarning("Intermediate property '{Property}' on path '{Path}' is null and cannot be auto-instantiated. Cannot set value.", properties[i], path);
                        return;
                    }
                }
                current = nextObj!;
            }
            var finalPropInfo = current.GetType().GetProperty(properties.Last(), System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (finalPropInfo != null && finalPropInfo.CanWrite)
            {
                try
                {
                    // Attempt type conversion if necessary
                    object? convertedValue = Convert.ChangeType(value, finalPropInfo.PropertyType);
                    finalPropInfo.SetValue(current, convertedValue);
                }
                catch (Exception ex)
                {
                     _logger.LogWarning(ex, "Failed to convert or set value for property '{Property}' on path '{Path}'. Value: {Value}", properties.Last(), path, value);
                }
            }
            else
            {
                _logger.LogWarning("Final property '{Property}' not found or not writable on path '{Path}'.", properties.Last(), path);
            }
        }


        public async Task LoadMappingRulesAsync(string mappingRuleId)
        {
            _logger.LogInformation("Loading mapping rules for '{MappingRuleId}'...", mappingRuleId);
            try
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, _mappingRulePathBase, "IoT", $"{mappingRuleId}.json");
                if (File.Exists(filePath))
                {
                    string jsonContent = await File.ReadAllTextAsync(filePath);
                    JsonDocument doc = JsonDocument.Parse(jsonContent);
                    _mappingRulesCache[mappingRuleId] = doc;
                    _logger.LogInformation("Successfully loaded and parsed mapping rules for '{MappingRuleId}' from {FilePath}", mappingRuleId, filePath);
                }
                else
                {
                    _logger.LogError("Mapping rule file not found for '{MappingRuleId}' at {FilePath}", mappingRuleId, filePath);
                    _mappingRulesCache.TryRemove(mappingRuleId, out _);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error loading mapping rules for '{MappingRuleId}'", mappingRuleId);
                _mappingRulesCache.TryRemove(mappingRuleId, out _); // Ensure corrupt/unloaded rules are removed
                throw;
            }
        }

        public bool AreRulesLoaded(string mappingRuleId)
        {
            return _mappingRulesCache.ContainsKey(mappingRuleId);
        }

        public async Task LoadAllConfiguredRulesAsync(IEnumerable<string> mappingRuleIds)
        {
             _logger.LogInformation("Loading all configured mapping rules...");
            foreach (var ruleId in mappingRuleIds.Distinct())
            {
                if (!string.IsNullOrEmpty(ruleId))
                {
                   await LoadMappingRulesAsync(ruleId);
                }
            }
             _logger.LogInformation("Finished loading configured mapping rules.");
        }
    }
}