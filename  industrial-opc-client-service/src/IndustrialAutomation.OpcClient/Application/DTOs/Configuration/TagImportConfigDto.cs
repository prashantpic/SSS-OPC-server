namespace IndustrialAutomation.OpcClient.Application.DTOs.Configuration
{
    public record TagImportConfigDto
    {
        public string FilePath { get; init; } = string.Empty;
        public string FileType { get; init; } = "Csv"; // "Csv", "Xml"
        public string DefaultServerId { get; init; } = string.Empty; // ServerId to assign if not specified in file
        public bool OverwriteExistingTags { get; init; } = false; // If true, tags with same TagId will be overwritten
    }
}