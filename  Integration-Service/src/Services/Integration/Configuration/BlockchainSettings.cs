namespace IntegrationService.Configuration
{
    public class CriticalDataCriteriaConfig
    {
        public string SourceProperty { get; set; } = string.Empty; // e.g., "TagId", "Value.Status"
        public string Operator { get; set; } = "Equals"; // e.g., "Equals", "Contains", "GreaterThan"
        public string Value { get; set; } = string.Empty;
    }

    public class BlockchainSettings
    {
        public const string SectionName = "Blockchain";

        public bool IsEnabled { get; set; } = false;
        public string RpcUrl { get; set; } = string.Empty;
        public long ChainId { get; set; }
        public string SmartContractAddress { get; set; } = string.Empty;
        public string SmartContractAbiPath { get; set; } = "Adapters/Blockchain/SmartContracts/CriticalDataLog.abi.json";
        public string CredentialIdentifier { get; set; } = string.Empty; // For the private key
        public string GasPriceStrategy { get; set; } = "Medium"; // e.g., "Low", "Medium", "High", or specific Gwei value
        public ulong MaxGasFee { get; set; } = 200000; // Max gas limit for transactions
        public List<CriticalDataCriteriaConfig> CriticalDataCriteria { get; set; } = new List<CriticalDataCriteriaConfig>();
    }
}