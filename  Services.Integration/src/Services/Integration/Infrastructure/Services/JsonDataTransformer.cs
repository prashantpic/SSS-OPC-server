using Opc.System.Services.Integration.Application.Contracts.Infrastructure;
using Opc.System.Services.Integration.Application.Contracts.Persistence;
using Opc.System.Services.Integration.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Opc.System.Services.Integration.Infrastructure.Services;

/// <summary>
/// Implements the IDataTransformer for JSON-based transformations using rules defined in a DataMap.
/// </summary>
public class JsonDataTransformer : IDataTransformer
{
    private readonly IDataMapRepository _dataMapRepository; // Assume repo exists

    public JsonDataTransformer(IDataMapRepository dataMapRepository)
    {
        _dataMapRepository = dataMapRepository;
    }

    /// <inheritdoc />
    public async Task<string> TransformAsync(Guid dataMapId, string sourcePayload)
    {
        var dataMap = await _dataMapRepository.GetByIdAsync(dataMapId);
        if (dataMap == null)
        {
            throw new ArgumentException($"DataMap with ID '{dataMapId}' not found.", nameof(dataMapId));
        }

        JsonNode? sourceNode;
        try
        {
            sourceNode = JsonNode.Parse(sourcePayload);
            if (sourceNode == null) throw new JsonException("Source payload parsed to null.");
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Source payload is not valid JSON.", nameof(sourcePayload), ex);
        }

        var transformationRules = ParseTransformationRules(dataMap.TransformationRules);
        var targetObject = new JsonObject();

        foreach (var rule in transformationRules)
        {
            var sourceValue = GetValueFromPath(sourceNode, rule.SourcePath);
            if (sourceValue != null)
            {
                // Clone the value to avoid adding a node that already has a parent
                var valueToSet = JsonNode.Parse(sourceValue.ToJsonString());
                SetValueFromPath(targetObject, rule.TargetPath, valueToSet);
            }
        }

        return targetObject.ToJsonString();
    }

    private IEnumerable<TransformationRule> ParseTransformationRules(JsonDocument rulesDocument)
    {
        return rulesDocument.RootElement.EnumerateArray()
            .Select(ruleElement => new TransformationRule
            {
                SourcePath = ruleElement.GetProperty("sourcePath").GetString() ?? string.Empty,
                TargetPath = ruleElement.GetProperty("targetPath").GetString() ?? string.Empty
            })
            .ToList();
    }

    private JsonNode? GetValueFromPath(JsonNode root, string path)
    {
        var pathSegments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments.Length == 0) return null;

        if (pathSegments[0] != "$") return null;

        JsonNode? currentNode = root;
        foreach (var segment in pathSegments.Skip(1))
        {
            if (currentNode is JsonObject jsonObject && jsonObject.ContainsKey(segment))
            {
                currentNode = jsonObject[segment];
            }
            else
            {
                return null;
            }
        }
        return currentNode;
    }

    private void SetValueFromPath(JsonObject root, string path, JsonNode? value)
    {
        var pathSegments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments.Length == 0) return;
        
        if (pathSegments[0] != "$") return;

        JsonObject currentNode = root;
        for (int i = 1; i < pathSegments.Length - 1; i++)
        {
            var segment = pathSegments[i];
            if (currentNode[segment] is not JsonObject nextNode)
            {
                nextNode = new JsonObject();
                currentNode[segment] = nextNode;
            }
            currentNode = nextNode;
        }

        var finalSegment = pathSegments.Last();
        currentNode[finalSegment] = value;
    }

    private class TransformationRule
    {
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
    }
}