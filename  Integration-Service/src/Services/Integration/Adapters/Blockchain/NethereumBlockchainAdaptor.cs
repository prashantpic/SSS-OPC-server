using IntegrationService.Adapters.Blockchain.Models;
using IntegrationService.Configuration;
using IntegrationService.Interfaces;
using IntegrationService.Resiliency;
using Microsoft.Extensions.Logging;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace IntegrationService.Adapters.Blockchain
{
    /// <summary>
    /// Blockchain adaptor using Nethereum library for Ethereum-compatible chains.
    /// </summary>
    public class NethereumBlockchainAdaptor : IBlockchainAdaptor, IDisposable
    {
        private readonly ILogger<NethereumBlockchainAdaptor> _logger;
        private readonly ICredentialManager _credentialManager;
        private readonly BlockchainSettings _config;
        private readonly RetryPolicy _retryPolicy;
        private readonly CircuitBreakerPolicy _circuitBreakerPolicy;

        private Web3? _web3;
        private Contract? _criticalDataLogContract;
        private Nethereum.Web3.Accounts.Account? _account;
        private string? _criticalDataLogAbi;

        public bool IsConnected => _web3 != null && _account != null; // Simplified check


        public NethereumBlockchainAdaptor(
            BlockchainSettings config, // Directly inject BlockchainSettings
            ILogger<NethereumBlockchainAdaptor> logger,
            ICredentialManager credentialManager,
            RetryPolicyFactory retryPolicyFactory,
            CircuitBreakerPolicyFactory circuitBreakerPolicyFactory)
        {
            _config = config;
            _logger = logger;
            _credentialManager = credentialManager;
            _retryPolicy = retryPolicyFactory.GetDefaultRetryPolicy(); 
            _circuitBreakerPolicy = circuitBreakerPolicyFactory.GetDefaultCircuitBreakerPolicy(); 

            _logger.LogInformation("NethereumBlockchainAdaptor initialized for RPC URL {RpcUrl} and Contract {ContractAddress}", _config.RpcUrl, _config.SmartContractAddress);
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                _logger.LogInformation("NethereumBlockchainAdaptor is already connected.");
                return;
            }
             _logger.LogInformation("Connecting NethereumBlockchainAdaptor to {RpcUrl}...", _config.RpcUrl);

            try
            {
                await _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        // Load private key and create account
                        var privateKey = await _credentialManager.GetCredentialAsync(_config.CredentialKey);
                        _account = new Nethereum.Web3.Accounts.Account(privateKey, _config.ChainId);

                        // Initialize Web3
                        _web3 = new Web3(_account, _config.RpcUrl);
                        _web3.TransactionManager.UseLegacyAsDefault = false; // For EIP-1559 support if chain uses it

                        // Load ABI if not already loaded
                        if (string.IsNullOrEmpty(_criticalDataLogAbi))
                        {
                            await LoadSmartContractAbiAsync();
                        }

                        // Get contract instance
                        if (!string.IsNullOrEmpty(_criticalDataLogAbi) && !string.IsNullOrEmpty(_config.SmartContractAddress))
                        {
                            _criticalDataLogContract = _web3.Eth.GetContract(_criticalDataLogAbi, _config.SmartContractAddress);
                            _logger.LogInformation("Nethereum Web3 client and contract instance initialized successfully for contract {ContractAddress}.", _config.SmartContractAddress);
                        }
                        else
                        {
                             _logger.LogError("ABI or Smart Contract Address is missing. Cannot initialize contract instance.");
                             throw new InvalidOperationException("ABI or Smart Contract Address is missing.");
                        }
                        
                        // Test connection (optional, e.g., get block number)
                        var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                        _logger.LogInformation("Successfully connected to blockchain. Current block number: {BlockNumber}", blockNumber.Value);

                    }));
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to connect NethereumBlockchainAdaptor after retries.");
                 _web3 = null; // Ensure IsConnected reflects failure
                 _account = null;
                 _criticalDataLogContract = null;
                 throw; 
            }
        }
        
        public Task DisconnectAsync()
        {
             _logger.LogInformation("NethereumBlockchainAdaptor Disconnect (typically no explicit disconnect needed for Web3).");
             // Nethereum's Web3 client doesn't manage a persistent connection in the same way
             // as MQTT or AMQP. Resources are managed per request.
             // Resetting internal state can be done if desired.
             // _web3 = null;
             // _account = null;
             // _criticalDataLogContract = null;
            return Task.CompletedTask;
        }


        public async Task<string> LogCriticalDataAsync(BlockchainTransactionRequest request)
        {
            if (_web3 == null || _criticalDataLogContract == null || _account == null || string.IsNullOrEmpty(_criticalDataLogAbi))
            {
                 _logger.LogWarning("Blockchain adaptor is not initialized. Cannot log data.");
                await ConnectAsync(); // Attempt to connect/initialize
                if (_web3 == null || _criticalDataLogContract == null) throw new InvalidOperationException("Blockchain adaptor failed to initialize.");
            }
             _logger.LogDebug("Preparing to log critical data to blockchain. SourceId: {SourceId}, DataPayload Hash: {DataPayload}", request.SourceId, request.DataPayload);

            // Determine which smart contract function to call
            // Default to 'logCriticalData' if not specified
            string functionName = !string.IsNullOrEmpty(request.FunctionName) ? request.FunctionName : "logCriticalData"; 
            var function = _criticalDataLogContract!.GetFunction(functionName);

            // Prepare transaction input
            // Parameters must match the smart contract function signature
            // Example: logCriticalData(string _sourceId, string _dataHash, uint256 _timestamp, string _metadata)
            var transactionInput = new TransactionInput
            {
                From = _account!.Address,
                To = _config.SmartContractAddress,
                ChainId = new HexBigInteger(_config.ChainId),
                // Gas and GasPrice can be estimated or set based on config
            };

            // Example parameters for "logCriticalData(string _sourceId, string _dataHash)"
            // Adjust parameters based on the actual smart contract ABI and request.FunctionName
            object[] functionParams;
            if (functionName == "logCriticalData")
            {
                // Assuming logCriticalData(string _sourceId, string _dataHash, uint256 _timestamp, string _metadata)
                functionParams = new object[] { request.SourceId, request.DataPayload, new BigInteger(request.Timestamp.ToUnixTimeSeconds()), request.Metadata };
            }
            else
            {
                _logger.LogError("Unsupported function name '{FunctionName}' for blockchain logging.", functionName);
                throw new NotSupportedException($"Function '{functionName}' is not supported for logging.");
            }


            try
            {
                 return await _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                        // Estimate gas price if configured
                        if (_config.GasPriceStrategy.Equals("Standard", StringComparison.OrdinalIgnoreCase) || _config.GasPriceStrategy.Equals("Fast", StringComparison.OrdinalIgnoreCase)) {
                            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync(); // Nethereum's default gas price estimator
                            transactionInput.GasPrice = gasPrice;
                             _logger.LogDebug("Estimated gas price: {GasPriceGwei} Gwei", Web3.Convert.FromWei(gasPrice.Value, Nethereum.Util.UnitConversion.EthUnit.Gwei));
                        }
                        
                        // Estimate gas limit
                        var gasLimit = await function.EstimateGasAsync(transactionInput.From, null, null, functionParams);
                        transactionInput.Gas = gasLimit; // Nethereum uses Gas for GasLimit in TransactionInput
                        _logger.LogDebug("Estimated gas limit: {GasLimit}", gasLimit.Value);


                        _logger.LogInformation("Sending blockchain transaction to {SmartContractAddress} (Function: {FunctionName}) on chain {ChainId} via '{RpcUrl}'. SourceId: {SourceId}",
                            _config.SmartContractAddress, functionName, _config.ChainId, _config.RpcUrl, request.SourceId);

                        var transactionReceipt = await function.SendTransactionAndWaitForReceiptAsync(
                            transactionInput.From, // From address
                            transactionInput.Gas, // Gas limit
                            transactionInput.GasPrice, // Gas price (can be null for EIP-1559 chains if MaxFeePerGas/MaxPriorityFeePerGas is set)
                            null, // Value (for sending ETH, typically 0 for contract calls)
                            CancellationToken.None, // CancellationToken
                            functionParams // Function parameters
                        );

                        if (transactionReceipt.Succeeded())
                        {
                            _logger.LogInformation("Blockchain transaction successful. Tx Hash: {TransactionHash}, Block: {BlockNumber}",
                                transactionReceipt.TransactionHash, transactionReceipt.BlockNumber.Value);
                            return transactionReceipt.TransactionHash;
                        }
                        else
                        {
                            _logger.LogError("Blockchain transaction failed. Tx Hash: {TransactionHash}, Status: {Status}, Gas Used: {GasUsed}. Revert Reason (if available): {RevertReason}",
                                transactionReceipt.TransactionHash, transactionReceipt.Status.Value, transactionReceipt.GasUsed.Value, transactionReceipt.RevertReason);
                            throw new Exception($"Blockchain transaction failed with status {transactionReceipt.Status.Value}. Tx Hash: {transactionReceipt.TransactionHash}. Revert Reason: {transactionReceipt.RevertReason}");
                        }
                    }));
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to send blockchain transaction after retries. SourceId: {SourceId}", request.SourceId);
                 throw; 
            }
        }

        public async Task<BlockchainRecord?> GetRecordAsync(string identifier)
        {
            if (_web3 == null || _criticalDataLogContract == null || _account == null || string.IsNullOrEmpty(_criticalDataLogAbi))
            {
                 _logger.LogWarning("Blockchain adaptor is not initialized. Cannot query record.");
                await ConnectAsync(); 
                if (_web3 == null || _criticalDataLogContract == null) throw new InvalidOperationException("Blockchain adaptor failed to initialize.");
            }
             _logger.LogDebug("Querying blockchain for record identifier: {Identifier}", identifier);

            try
            {
                 var transactionHash = identifier; 
                 _logger.LogDebug("Attempting to get transaction receipt for hash: {TransactionHash}", transactionHash);

                 var receipt = await _retryPolicy.ExecuteAsync(() =>
                    _circuitBreakerPolicy.ExecuteAsync(async () =>
                    {
                         var txReceipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                         if (txReceipt == null) throw new Exception($"Transaction receipt not found for hash {transactionHash}.");
                         return txReceipt;
                    }));

                 if (receipt != null && receipt.Succeeded())
                 {
                     _logger.LogDebug("Successfully retrieved transaction receipt. Parsing logs...");
                     
                     // Example: Assuming an event "CriticalDataLogged(string sourceId, string dataHash, uint256 timestamp, string metadata)"
                     var eventLogs = receipt.DecodeAllEvents<CriticalDataLoggedEventDTO>(); // Define this DTO based on ABI
                     var relevantLog = eventLogs.FirstOrDefault(); // Assuming one relevant event per transaction

                     if (relevantLog != null)
                     {
                         // Get block timestamp
                         var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(receipt.BlockNumber);
                         DateTimeOffset blockTimestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value);

                         return new BlockchainRecord
                         {
                             TransactionHash = receipt.TransactionHash,
                             BlockNumber = (ulong)receipt.BlockNumber.Value,
                             Timestamp = blockTimestamp, 
                             DataPayload = relevantLog.Event.DataHash, 
                             Metadata = relevantLog.Event.Metadata 
                         };
                     } else {
                          _logger.LogWarning("No 'CriticalDataLoggedEventDTO' (or similar) event found in transaction receipt {TransactionHash}.", transactionHash);
                     }
                     
                     return new BlockchainRecord // Fallback with minimal info if event parsing fails
                     {
                         TransactionHash = receipt.TransactionHash,
                         BlockNumber = (ulong)receipt.BlockNumber.Value,
                         Timestamp = DateTimeOffset.UtcNow, // Placeholder, needs block lookup
                         DataPayload = "Data from Tx Input (Not Parsed)" 
                     };
                 }

                 _logger.LogWarning("Transaction receipt not found or transaction failed for identifier {Identifier}.", identifier);
                 return null; 

            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Failed to query blockchain record for identifier {Identifier} after retries.", identifier);
                 throw; 
            }
        }

         private async Task LoadSmartContractAbiAsync()
        {
            var abiPath = Path.Combine(AppContext.BaseDirectory, "Adapters", "Blockchain", "SmartContracts", "CriticalDataLog.abi.json");
            if (File.Exists(abiPath))
            {
                _criticalDataLogAbi = await File.ReadAllTextAsync(abiPath);
                 _logger.LogInformation("Successfully loaded smart contract ABI from {AbiPath}", abiPath);
            }
            else
            {
                 _logger.LogError("Smart contract ABI file not found at {AbiPath}.", abiPath);
                throw new FileNotFoundException($"Smart contract ABI file not found at {abiPath}.");
            }
        }

         public void Dispose()
        {
             _logger.LogInformation("NethereumBlockchainAdaptor Dispose called.");
             GC.SuppressFinalize(this);
        }
    }

    // Example DTO for decoding "CriticalDataLogged" event
    // This should match the event signature in your smart contract ABI
    [Event("CriticalDataLogged")]
    public class CriticalDataLoggedEventDTO : IEventDTO
    {
        [Parameter("string", "sourceId", 1, true)] // Indexed parameters are marked true
        public string SourceId { get; set; } = string.Empty;

        [Parameter("string", "dataHash", 2, false)]
        public string DataHash { get; set; } = string.Empty;

        [Parameter("uint256", "timestamp", 3, false)]
        public BigInteger Timestamp { get; set; }

        [Parameter("string", "metadata", 4, false)]
        public string Metadata { get; set; } = string.Empty;
    }
}