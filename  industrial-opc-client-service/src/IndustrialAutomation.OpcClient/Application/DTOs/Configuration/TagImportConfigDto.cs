namespace IndustrialAutomation.OpcClient.Application.DTOs.Configuration
{
    public record TagImportConfigDto(
        string FilePath,
        string FileType, // "CSV", "XML"
        string ServerId // Optional: Default server ID to associate tags with if not specified in file
    );
}