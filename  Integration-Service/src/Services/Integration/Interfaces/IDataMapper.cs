using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntegrationService.Interfaces
{
    /// <summary>
    /// Interface for data mapping services.
    /// Defines a contract for transforming data between different schemas, 
    /// specifically from an internal/OPC data model to the schema required by an 
    /// external target system (e.g., IoT platform, Digital Twin) and vice-versa, 
    /// based on configurable rules.
    /// </summary>
    public interface IDataMapper
    {
        /// <summary>
        /// Transforms data from a source format to a target format based on mapping rules.
        /// </summary>
        /// <typeparam name="TSource">The type of the source data.</typeparam>
        /// <typeparam name="TTarget">The target data type.</typeparam>
        /// <param name="sourceData">The data to transform.</param>
        /// <param name="mappingRuleId">The identifier for the mapping rules to use.</param>
        /// <returns>The transformed data in the target format.</returns>
        TTarget Map<TSource, TTarget>(TSource sourceData, string mappingRuleId) where TTarget : new();

        /// <summary>
        /// Asynchronously loads or reloads mapping rules for a specific rule ID.
        /// Implementations might fetch rules from files, a database, or a configuration service.
        /// </summary>
        /// <param name="mappingRuleId">The identifier of the rules to load.</param>
        Task LoadMappingRulesAsync(string mappingRuleId);

        /// <summary>
        /// Checks if mapping rules for a given ID are currently loaded and available.
        /// </summary>
        /// <param name="mappingRuleId">The identifier for the mapping rules.</param>
        /// <returns>True if rules are loaded, false otherwise.</returns>
        bool AreRulesLoaded(string mappingRuleId);

        /// <summary>
        /// Asynchronously loads all mapping rules specified in a list of rule IDs.
        /// This can be used at startup to pre-load all necessary rules.
        /// </summary>
        /// <param name="mappingRuleIds">An enumerable collection of mapping rule IDs to load.</param>
        Task LoadAllConfiguredRulesAsync(IEnumerable<string> mappingRuleIds);
    }
}