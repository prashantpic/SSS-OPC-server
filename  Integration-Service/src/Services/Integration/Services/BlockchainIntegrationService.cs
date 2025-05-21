namespace IntegrationService.Services
{
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using IntegrationService.Adapters.Blockchain.Models; // Assuming opcData is a complex object or JSON
    using IntegrationService.Configuration;
    using IntegrationService.Interfaces;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class BlockchainIntegrationService
    {
        private readonly ILogger<BlockchainIntegrationService> _logger;
        private readonly IOptionsMonitor<BlockchainSettings> _blockchainSettings;
        private readonly IBlockchainAdaptor _blockchainAdaptor;
        private readonly ICredentialManager _credentialManager; // Used by NethereumBlockchainAdaptor

        public BlockchainIntegrationService(
            ILogger<BlockchainIntegrationService> logger,
            IOptionsMonitor<BlockchainSettings> blockchainSettings,
            IBlockchainAdaptor blockchainAdaptor,
            ICredentialManager credentialManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blockchainSettings = blockchainSettings ?? throw new ArgumentNullException(nameof(blockchainSettings));
            _blockchainAdaptor = blockchainAdaptor ?? throw new ArgumentNullException(nameof(blockchainAdaptor));
            _credentialManager = credentialManager; // Injected but primarily used by the adaptor itself
        }

        public async Task LogDataIfCriticalAsync(object data, string sourceId, CancellationToken cancellationToken)
        {
            var settings = _blockchainSettings.CurrentValue;
            if (!settings.IsEnabled)
            {
                _logger.LogDebug("Blockchain integration is disabled. Skipping data logging.");
                return;
            }

            if (!IsDataCritical(data, settings.CriticalDataCriteria))
            {
                _logger.LogTrace("Data from source {SourceId} does not meet critical criteria for blockchain logging.", sourceId);
                return;
            }

            _logger.LogInformation("Data from source {SourceId} meets critical criteria. Preparing for blockchain logging.", sourceId);

            try
            {
                string dataJson = JsonSerializer.Serialize(data);
                string dataHash = ComputeSha256Hash(dataJson);
                var timestamp = DateTimeOffset.UtcNow;

                var request = new BlockchainTransactionRequest(
                    DataHash: dataHash,
                    SourceId: sourceId,
                    Timestamp: timestamp
                // SmartContractParameters can be added here if the contract requires more than hash, source, and time
                );

                // The adaptor is responsible for using ICredentialManager to get the private key
                // and for handling asynchronous submission to the blockchain.
                var record = await _blockchainAdaptor.LogCriticalDataAsync(request, cancellationToken);

                if (record != null)
                {
                    _logger.LogInformation(
                        "Successfully submitted data to blockchain. Source: {SourceId}, DataHash: {DataHash}, TxHash: {TransactionHash}, Block: {BlockNumber}",
                        sourceId, dataHash, record.TransactionHash, record.BlockNumber);
                }
                else
                {
                    _logger.LogWarning("Blockchain logging initiated for Source: {SourceId}, DataHash: {DataHash}, but transaction receipt/record was not immediately available or processing failed upstream in adaptor.", sourceId, dataHash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging critical data to blockchain for source {SourceId}.", sourceId);
                // Potentially re-queue or store for later retry if the error is transient and adaptor doesn't handle it.
            }
        }

        private bool IsDataCritical(object data, List<CriticalDataCriteriaConfig> criteriaList)
        {
            if (!criteriaList.Any())
            {
                _logger.LogDebug("No critical data criteria defined. Assuming all data passed for logging is critical.");
                return true; // If no criteria, assume any data passed to this service is intended to be logged.
            }

            // This is a simplified criteria checker. A more robust solution might involve
            // a rules engine or more complex expression evaluation.
            // For this example, we'll assume data is a Dictionary<string, object> or can be serialized to one.
            // Or use System.Text.Json.Nodes.JsonObject for inspection.
            JsonDocument? dataDoc = null;
            try
            {
                if (data is JsonElement element)
                {
                    dataDoc = JsonDocument.Parse(element.GetRawText());
                }
                else
                {
                    dataDoc = JsonDocument.Parse(JsonSerializer.Serialize(data));
                }

                if (dataDoc == null) return false;


                foreach (var criteria in criteriaList)
                {
                    if (!dataDoc.RootElement.TryGetProperty(criteria.SourceProperty, out JsonElement propertyValue))
                    {
                        continue; // Property not found, this criterion doesn't match.
                    }

                    string? valToCompare = propertyValue.ValueKind switch
                    {
                        JsonValueKind.String => propertyValue.GetString(),
                        JsonValueKind.Number => propertyValue.GetRawText(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        _ => propertyValue.ToString()
                    };


                    bool match = criteria.Operator.ToLowerInvariant() switch
                    {
                        "equals" => string.Equals(valToCompare, criteria.Value, StringComparison.OrdinalIgnoreCase),
                        "contains" => valToCompare?.Contains(criteria.Value, StringComparison.OrdinalIgnoreCase) ?? false,
                        // Add more operators as needed (e.g., GreaterThan, LessThan for numeric)
                        _ => false
                    };

                    if (match) return true; // If any criterion matches, it's critical.
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Could not parse data for critical check. Assuming not critical.");
                return false;
            }
            finally
            {
                dataDoc?.Dispose();
            }


            return false; // No criteria matched.
        }

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}