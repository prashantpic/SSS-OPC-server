using IntegrationService.Configuration.Models;
using System.Collections.Generic;

namespace IntegrationService.Configuration
{
    /// <summary>
    /// Configuration settings for Blockchain integration.
    /// </summary>
    public class BlockchainSettings
    {
        /// <summary>
        /// Blockchain network details (e.g., RPC URL).
        /// </summary>
        public string RpcUrl { get; set; } = string.Empty;

        /// <summary>
        /// Chain ID for the blockchain network.
        /// </summary>
        public int ChainId { get; set; }

        /// <summary>
        /// Smart contract address for data logging.
        /// </summary>
        public string SmartContractAddress { get; set; } = string.Empty;

        /// <summary>
        /// Credential key to look up the private key in ICredentialManager.
        /// </summary>
        public string CredentialKey { get; set; } = string.Empty;

        /// <summary>
        /// Gas price strategy (e.g., Standard, Fast).
        /// </summary>
        public string GasPriceStrategy { get; set; } = "Standard";

        /// <summary>
        /// Criteria for determining what data is considered critical for logging.
        /// </summary>
        public CriticalDataCriteriaSettings CriticalDataCriteria { get; set; } = new CriticalDataCriteriaSettings();
    }

     namespace Models
    {
        public class CriticalDataCriteriaSettings
        {
            public bool Enabled { get; set; }
            public List<string> OpcTagMatchPatterns { get; set; } = new List<string>();
            public string MinSeverityLevel { get; set; } = "Warning"; // e.g., Information, Warning, Error
        }
    }
}