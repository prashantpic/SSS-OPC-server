namespace IntegrationService.Configuration
{
    public class IoTPlatformSettings
    {
        public const string SectionName = "IoTPlatforms";

        public List<IoTPlatformConfig> Platforms { get; set; } = new List<IoTPlatformConfig>();
    }
}