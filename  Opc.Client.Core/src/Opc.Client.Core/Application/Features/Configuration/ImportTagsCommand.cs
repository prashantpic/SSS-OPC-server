namespace Opc.Client.Core.Application.Features.Configuration;

/// <summary>
/// Represents a use case for importing tag configurations from a file.
/// </summary>
/// <remarks>
/// This is a data-carrying object used in a CQRS pattern. It encapsulates all the necessary
/// information for a command handler to execute the tag import process.
/// </remarks>
/// <param name="FilePath">The full path to the file containing tag configurations.</param>
/// <param name="FileFormat">The format of the import file (e.g., CSV, XML).</param>
public record ImportTagsCommand(string FilePath, FileFormat FileFormat);

/// <summary>
/// Defines the supported file formats for tag import.
/// </summary>
public enum FileFormat
{
    /// <summary>
    /// Comma-Separated Values format.
    /// </summary>
    Csv,
    
    /// <summary>
    /// Extensible Markup Language format.
    /// </summary>
    Xml
}