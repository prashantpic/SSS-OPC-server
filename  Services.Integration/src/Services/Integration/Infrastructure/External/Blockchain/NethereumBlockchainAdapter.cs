using Microsoft.Extensions.Logging;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Opc.System.Services.Integration.Application.Contracts.External;
using Opc.System.Services.Integration.Domain.Aggregates;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using Opc.System.Services.Integration.Application.Contracts.Persistence; // Assuming this contract exists
using Nethereum.Hex.HexTypes;
using Nethereum.Contracts;

namespace Opc.System.Services.Integration.Infrastructure.External.Blockchain;

/// <summary>
/// Implementation of IBlockchainAdapter using the Nethereum library for Ethereum-based blockchains.
/// </summary>
public class NethereumBlockchainAdapter : IBlockchainAdapter
{
    private readonly IIntegrationConnectionRepository _connectionRepository;
    private readonly ILogger<NethereumBlockchainAdapter> _logger;
    private const string ContractAbi = @"[{""constant"":false,""inputs"":[{""name"":""dataHash"",""type"":""bytes32""},{""name"":""metadata"",""type"":""string""}],""name"":""logTransaction"",""outputs"":[],""payable"":false,""stateMutability"":""nonpayable"",""type"":""function""},{""constant"":true,""inputs"":[{""name"":""dataHash"",""type"":""bytes32""}],""name"":""getTransaction"",""outputs"":[{""name"":"""",""type"":""string""},{""name"":"""",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""}]";

    public NethereumBlockchainAdapter(IIntegrationConnectionRepository connectionRepository, ILogger<NethereumBlockchainAdapter> logger)
    {
        _connectionRepository = connectionRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> LogDataHashAsync(Guid connectionId, string dataHash, string metadata)
    {
        var connection = await _connectionRepository.GetByIdAsync(connectionId);
        if (connection == null || !connection.IsEnabled)
        {
            _logger.LogWarning("Blockchain connection {ConnectionId} not found or is disabled. Cannot log data hash.", connectionId);
            throw new InvalidOperationException($"Blockchain connection {connectionId} is not available.");
        }

        try
        {
            var config = GetSecurityConfig(connection);
            var account = new Account(config.PrivateKey, config.ChainId);
            var web3 = new Web3(account, config.NodeUrl);
            
            var contract = web3.Eth.GetContract(ContractAbi, config.SmartContractAddress);
            var logFunction = contract.GetFunction("logTransaction");
            
            // Convert string hash to bytes32
            var dataHashBytes = new byte[32];
            var hashBytes = Convert.FromHexString(dataHash);
            Array.Copy(hashBytes, 0, dataHashBytes, 0, hashBytes.Length);

            _logger.LogInformation("Sending transaction to smart contract {ContractAddress} on chain {ChainId}", config.SmartContractAddress, config.ChainId);

            var transactionHash = await logFunction.SendTransactionAsync(account.Address, new HexBigInteger(300000), null, dataHashBytes, metadata);
            
            _logger.LogInformation("Transaction sent successfully. Transaction Hash: {TransactionHash}", transactionHash);
            
            return transactionHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log data hash to blockchain for connection {ConnectionId}", connectionId);
            throw; // Rethrow to allow caller (e.g., CQRS handler) to handle the failure
        }
    }

    /// <inheritdoc />
    public Task<(bool IsValid, string Metadata)> VerifyTransactionByHashAsync(Guid connectionId, string dataHash)
    {
        _logger.LogWarning("VerifyTransactionByHashAsync is not fully implemented.");
        // A full implementation would retrieve the connection, create a web3 client,
        // get the contract handle, and call a 'view' or 'pure' function like 'getTransaction(bytes32 dataHash)'
        // to retrieve the stored metadata and timestamp, then validate it.
        return Task.FromResult((false, string.Empty));
    }
    
    private BlockchainSecurityConfiguration GetSecurityConfig(IntegrationConnection connection)
    {
        var config = JsonSerializer.Deserialize<BlockchainSecurityConfiguration>(connection.SecurityConfiguration.RootElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (config == null || string.IsNullOrWhiteSpace(config.NodeUrl) || string.IsNullOrWhiteSpace(config.PrivateKey) || string.IsNullOrWhiteSpace(config.SmartContractAddress))
        {
            throw new InvalidOperationException($"Invalid or incomplete security configuration for blockchain connection {connection.Id}");
        }
        return config;
    }
    
    private class BlockchainSecurityConfiguration
    {
        public string NodeUrl { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string SmartContractAddress { get; set; } = string.Empty;
        public long ChainId { get; set; }
    }
}