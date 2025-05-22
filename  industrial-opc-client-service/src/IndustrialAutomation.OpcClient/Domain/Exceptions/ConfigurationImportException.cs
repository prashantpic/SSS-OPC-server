using System;

namespace IndustrialAutomation.OpcClient.Domain.Exceptions
{
    /// <summary>
    /// Specific exception type for errors encountered while importing configurations, 
    /// such as invalid file format or data.
    /// </summary>
    public class ConfigurationImportException : Exception
    {
        /// <summary>
        /// Path to the configuration file that caused the import error, if applicable.
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Additional details about the error encountered during import.
        /// </summary>
        public string? ErrorDetails { get; }

        public ConfigurationImportException()
        {
        }

        public ConfigurationImportException(string message) : base(message)
        {
        }

        public ConfigurationImportException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConfigurationImportException(string message, string? filePath, string? errorDetails = null, Exception? innerException = null)
            : base(message, innerException)
        {
            FilePath = filePath;
            ErrorDetails = errorDetails;
        }
    }
}