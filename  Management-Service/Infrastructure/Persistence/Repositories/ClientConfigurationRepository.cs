using ManagementService.Application.Abstractions.Clients;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Infrastructure.Persistence.Repositories;

public class ClientConfigurationRepository : IClientConfigurationRepository
{
    private readonly ManagementDbContext _context;

    public ClientConfigurationRepository(ManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ClientConfiguration?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ClientConfigurations
                             .Include(cc => cc.Versions)
                             .FirstOrDefaultAsync(cc => cc.Id == id, cancellationToken);
    }

    public async Task<ClientConfiguration?> GetByClientInstanceIdAsync(Guid clientInstanceId, CancellationToken cancellationToken)
    {
        return await _context.ClientConfigurations
                             .Include(cc => cc.Versions)
                             .FirstOrDefaultAsync(cc => cc.ClientInstanceId == clientInstanceId, cancellationToken);
    }

    public async Task<ClientConfiguration?> GetByIdWithVersionAsync(Guid configId, Guid versionId, CancellationToken cancellationToken)
    {
        // Eager load all versions, then check if the specific version exists.
        // This is because ConfigurationVersion is owned and cannot be directly queried by ID easily
        // without knowing its parent.
        var config = await _context.ClientConfigurations
                                   .Include(cc => cc.Versions)
                                   .FirstOrDefaultAsync(cc => cc.Id == configId, cancellationToken);

        if (config != null && config.Versions.Any(v => v.Id == versionId))
        {
            return config;
        }
        return null;
    }


    public async Task AddAsync(ClientConfiguration clientConfiguration, CancellationToken cancellationToken)
    {
        await _context.ClientConfigurations.AddAsync(clientConfiguration, cancellationToken);
    }

    public Task UpdateAsync(ClientConfiguration clientConfiguration, CancellationToken cancellationToken)
    {
        _context.ClientConfigurations.Update(clientConfiguration);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var configToDelete = await _context.ClientConfigurations
                                           .Include(c => c.Versions) // Need to include owned entities for cascading delete if configured, or manual removal
                                           .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (configToDelete != null)
        {
            // If versions are owned and configured for cascade delete, EF Core handles it.
            // Otherwise, you might need to clear them or handle removal explicitly if cascade is not set up.
            // configToDelete.ClearVersions(); // Example if manual removal of owned items is needed
            _context.ClientConfigurations.Remove(configToDelete);
        }
    }

    public async Task<ConfigurationVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken)
    {
        // Query across all configurations to find one that owns this version.
        // This can be inefficient if there are many configurations and versions.
        // A better approach might involve a direct SQL query or a different model if this query is frequent and slow.
        var version = await _context.ClientConfigurations
                                    .SelectMany(cc => cc.Versions)
                                    .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);
        return version;
    }
}