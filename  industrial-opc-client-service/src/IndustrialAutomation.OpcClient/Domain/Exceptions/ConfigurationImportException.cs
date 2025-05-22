using System;

namespace IndustrialAutomation.OpcClient.Domain.Exceptions;

/// <summary>
/// Specific exception type for errors encountered while importing configurations, 
/// such as invalid file format or data.
/// </summary>
public class ConfigurationImportException : Exception
{
    public string? FilePath { get; }
    public string? ErrorDetails { get; }

    public ConfigurationImportException(string message, string? filePath = null, string? errorDetails = null, Exception? innerException = null)
        : base(message, innerException)
    {
        FilePath = filePath;
        ErrorDetails = errorDetails;
    }
    
    public ConfigurationImportException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ConfigurationImportException(string message)
        : base(message)
    {
    }
}