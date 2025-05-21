using System.Collections.Generic;

namespace IntegrationService.Configuration
{
    /// <summary>
    /// Configuration settings specific to IoT Platform integrations.
    /// </summary>
    public class IoTPlatformSettings
    {
        /// <summary>
        /// Contains a list or dictionary of individual IoT platform configurations.
        /// Allows defining multiple IoT platform targets (e.g., AWS IoT, Azure IoT Hub).
        /// </summary>
        public List<IoTPlatformConfig> Platforms { get; set; } = new List<IoTPlatformConfig>();
    }
}