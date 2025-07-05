using Opc.System.BackgroundWorkers.DataLifecycle.Application.Interfaces;
using Opc.System.BackgroundWorkers.DataLifecycle.Domain.Entities;
using Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure.Persistence;

namespace Opc.System.BackgroundWorkers.DataLifecycle.Infrastructure;

/// <summary>
/// Implements the IPolicyProvider interface. It interacts with a repository
/// to fetch data retention policy configurations from the database.
/// </summary>
public sealed class PolicyProvider : IPolicyProvider
{
    private readonly IDataRetentionPolicyRepository _policyRepository;
    private readonly ILogger<PolicyProvider> _logger;

    public PolicyProvider(IDataRetentionPolicyRepository policyRepository, ILogger<PolicyProvider> logger)
    {
        _policyRepository = policyRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataRetentionPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Fetching active data retention policies.");
            var policies = await _policyRepository.GetActivePoliciesAsync(cancellationToken);
            _logger.LogInformation("Successfully fetched {Count} active policies.", policies.Count());
            return policies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch data retention policies from the repository.");
            // Return an empty list to prevent the job from crashing
            return Enumerable.Empty<DataRetentionPolicy>();
        }
    }
}