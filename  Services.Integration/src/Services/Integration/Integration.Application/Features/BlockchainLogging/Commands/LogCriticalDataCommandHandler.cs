using MediatR;
using Microsoft.Extensions.Logging;
using Opc.System.Services.Integration.Application.Contracts.External;
using System.Security.Cryptography;
using System.Text;

namespace Opc.System.Services.Integration.Application.Features.BlockchainLogging.Commands;

/// <summary>
/// Command to request logging of a critical data payload to the blockchain.
/// </summary>
/// <param name="ConnectionId">The ID of the blockchain connection configuration to use.</param>
/// <param name="DataPayload">The raw data payload of the critical event.</param>
/// <param name="Metadata">Additional metadata to be stored on-chain with the data hash.</param>
public record LogCriticalDataCommand(Guid ConnectionId, string DataPayload, string Metadata) : IRequest<string>;

/// <summary>
/// Handles the <see cref="LogCriticalDataCommand"/> to log critical data asynchronously to the blockchain.
/// </summary>
public class LogCriticalDataCommandHandler : IRequestHandler<LogCriticalDataCommand, string>
{
    private readonly IBlockchainAdapter _blockchainAdapter;
    private readonly ILogger<LogCriticalDataCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogCriticalDataCommandHandler"/> class.
    /// </summary>
    /// <param name="blockchainAdapter">The adapter for blockchain interactions.</param>
    /// <param name="logger">The logger for capturing operational information.</param>
    public LogCriticalDataCommandHandler(IBlockchainAdapter blockchainAdapter, ILogger<LogCriticalDataCommandHandler> logger)
    {
        _blockchainAdapter = blockchainAdapter;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command by hashing the data payload and sending it to the blockchain adapter.
    /// </summary>
    /// <param name="request">The command containing the data to log.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The transaction ID from the blockchain transaction.</returns>
    public async Task<string> Handle(LogCriticalDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Hash the incoming data payload to get a unique, fixed-size fingerprint.
        var dataHash = HashData(request.DataPayload);

        // 2. Call the blockchain adapter to log the hash and metadata.
        _logger.LogInformation("Logging hash {DataHash} to blockchain for connection {ConnectionId}", dataHash, request.ConnectionId);
        
        var transactionId = await _blockchainAdapter.LogDataHashAsync(request.ConnectionId, dataHash, request.Metadata);
        
        _logger.LogInformation("Successfully logged data hash {DataHash} to blockchain with transaction ID {TransactionId}", dataHash, transactionId);

        // Note: The full DataPayload is stored off-chain by another service (e.g., Data Service).
        // This service is only responsible for anchoring the hash on the blockchain for tamper-evidence.

        return transactionId;
    }

    /// <summary>
    /// Computes a SHA256 hash for the given string data.
    /// </summary>
    /// <param name="data">The string to hash.</param>
    /// <returns>A lowercase hexadecimal representation of the hash.</returns>
    private string HashData(string data)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}