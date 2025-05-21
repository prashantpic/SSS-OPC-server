using System.Collections.Generic;

namespace IntegrationService.Configuration
{
    /// <summary>
    /// Configuration settings specific to Digital Twin integrations.
    /// </summary>
    public class DigitalTwinSettings
    {
        /// <summary>
        /// Contains a list or dictionary of individual Digital Twin platform configurations.
        /// Allows defining multiple Digital Twin integration targets.
        /// </summary>
        public List<DigitalTwinConfig> Twins { get; set; } = new List<DigitalTwinConfig>();
    }
}