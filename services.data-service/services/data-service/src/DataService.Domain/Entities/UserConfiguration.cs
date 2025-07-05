using System;

namespace DataService.Domain.Entities
{
    /// <summary>
    /// Represents a key-value configuration setting, stored in the relational database.
    /// This entity is used for storing user preferences, system settings, and other configuration data.
    /// Fulfills requirement REQ-DLP-008 for secure configuration storage.
    /// </summary>
    public class UserConfiguration
    {
        /// <summary>
        /// The unique identifier for the configuration entry.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The unique key identifying the configuration setting.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The value of the configuration setting. Can be encrypted.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Optional metadata describing the type of the value (e.g., "string", "int", "json").
        /// </summary>
        public string? DataType { get; set; }

        /// <summary>
        /// A flag indicating whether the 'Value' field is encrypted in the database.
        /// </summary>
        public bool IsEncrypted { get; set; }
    }
}