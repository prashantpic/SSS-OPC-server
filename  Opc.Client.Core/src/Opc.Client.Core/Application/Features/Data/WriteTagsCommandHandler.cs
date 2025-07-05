using Opc.Client.Core.Application.Interfaces;

namespace Opc.Client.Core.Application.Features.Data;

/// <summary>
/// Orchestrates the entire process of writing tag values, including validation, limiting, and auditing.
/// </summary>
/// <remarks>
/// This handler follows the CQRS pattern. It coordinates multiple services to enforce business
/// rules before dispatching the write command to the underlying OPC protocol client.
/// </remarks>
public class WriteTagsCommandHandler
{
    private readonly IOpcClientManager _clientManager;
    private readonly IWriteValidator _validator;
    private readonly IWriteRateLimiter _rateLimiter;
    private readonly ICriticalWriteAuditor _criticalLogger;

    public WriteTagsCommandHandler(
        IOpcClientManager clientManager,
        IWriteValidator validator,
        IWriteRateLimiter rateLimiter,
        ICriticalWriteAuditor criticalLogger)
    {
        _clientManager = clientManager;
        _validator = validator;
        _rateLimiter = rateLimiter;
        _criticalLogger = criticalLogger;
    }

    /// <summary>
    /// Handles the command to write values to OPC tags.
    /// </summary>
    /// <param name="command">The command containing write details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The result of the write operation.</returns>
    public async Task<WriteResult> Handle(WriteTagsCommand command, CancellationToken cancellationToken)
    {
        // 1. Check rate limiter
        if (!_rateLimiter.IsActionAllowed(command.UserId))
        {
            return new WriteResult(false, "Write operation rate limit exceeded.", null);
        }

        // 2. Validate data against business rules
        var validationResult = _validator.Validate(command.ValuesToWrite);
        if (!validationResult.IsValid)
        {
            return new WriteResult(false, $"Validation failed: {validationResult.ErrorMessage}", null);
        }
        
        var client = _clientManager.GetClient(command.ServerId);
        if (client == null)
        {
            return new WriteResult(false, $"No active client found for ServerId: {command.ServerId}", null);
        }

        // 3. Pre-fetch old values for critical tags for auditing
        IDictionary<string, object?>? oldValues = null;
        if (command.IsCritical)
        {
            oldValues = await _criticalLogger.PreFetchCriticalValuesAsync(client, command.ValuesToWrite.Keys);
        }

        // 4. Delegate the actual write to the protocol client
        var writeResult = await client.WriteAsync(command.ValuesToWrite, cancellationToken);
        
        // 5. Log the critical write operation asynchronously (fire and forget)
        if (command.IsCritical && oldValues != null)
        {
            _ = _criticalLogger.LogCriticalWriteAsync(command, writeResult, oldValues);
        }

        return writeResult;
    }
}


// --- Placeholder types and interfaces for compilation ---

public record WriteTagsCommand(
    Guid ServerId, 
    string UserId, 
    bool IsCritical, 
    IDictionary<string, object> ValuesToWrite
);

public interface IOpcClientManager
{
    IOpcProtocolClient? GetClient(Guid serverId);
}

public interface IWriteValidator
{
    ValidationResult Validate(IDictionary<string, object> values);
}
public record ValidationResult(bool IsValid, string? ErrorMessage);

public interface IWriteRateLimiter
{
    bool IsActionAllowed(string userId);
}

public interface ICriticalWriteAuditor
{
    Task<IDictionary<string, object?>> PreFetchCriticalValuesAsync(IOpcProtocolClient client, IEnumerable<string> nodeIds);
    Task LogCriticalWriteAsync(WriteTagsCommand command, WriteResult result, IDictionary<string, object?> oldValues);
}