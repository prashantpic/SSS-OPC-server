namespace IntegrationService.Configuration
{
    public class DigitalTwinSettings
    {
        public const string SectionName = "DigitalTwins";
        public List<DigitalTwinConfig> Platforms { get; set; } = new List<DigitalTwinConfig>();
    }
}