using System;

namespace Opc.System.Services.Management.Domain.Aggregates
{
    /// <summary>
    /// Models a plan for migrating configurations from a legacy system.
    /// Encapsulates the rules and procedures for migrating client configurations from a specific source system.
    /// </summary>
    public class MigrationStrategy
    {
        /// <summary>
        /// Unique identifier for the migration strategy.
        /// </summary>
        public MigrationStrategyId Id { get; private set; }

        /// <summary>
        /// Name of the legacy system (e.g., "LegacyKepwareCsv").
        /// </summary>
        public string SourceSystem { get; private set; }

        /// <summary>
        /// JSON string defining how to map legacy fields to the new ClientConfiguration model.
        /// </summary>
        public string MappingRules { get; private set; }

        /// <summary>
        /// A script or description of validation steps.
        /// </summary>
        public string ValidationScript { get; private set; }

        /// <summary>
        /// Private constructor for EF Core.
        /// </summary>
        private MigrationStrategy() { }

        /// <summary>
        /// Creates a new instance of a MigrationStrategy.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <param name="sourceSystem">The source legacy system.</param>
        /// <param name="mappingRules">The JSON mapping rules.</param>
        /// <param name="validationScript">The validation script or description.</param>
        public MigrationStrategy(MigrationStrategyId id, string sourceSystem, string mappingRules, string validationScript)
        {
            Id = id;
            SourceSystem = sourceSystem;
            MappingRules = mappingRules;
            ValidationScript = validationScript;
        }
    }

    /// <summary>
    /// Strongly-typed identifier for MigrationStrategy.
    /// </summary>
    /// <param name="Value">The underlying Guid value.</param>
    public readonly record struct MigrationStrategyId(Guid Value);
}