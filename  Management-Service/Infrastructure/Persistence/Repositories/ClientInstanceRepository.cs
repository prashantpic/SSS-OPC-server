using ManagementService.Application.Abstractions.Clients;
using ManagementService.Domain.Aggregates.ClientInstanceAggregate;
using ManagementService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Infrastructure.Persistence.Repositories;

public class ClientInstanceRepository : IClientInstanceRepository
{
    private readonly ManagementDbContext _context;

    public ClientInstanceRepository(ManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ClientInstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ClientInstances
                             .Include(ci => ci.ClientConfiguration)
                                .ThenInclude(cc => cc!.Versions) // Eager load versions of the config
                             .FirstOrDefaultAsync(ci => ci.Id == id, cancellationToken);
    }

    public async Task<ClientInstance?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.ClientInstances
                             .Include(ci => ci.ClientConfiguration)
                                .ThenInclude(cc => cc!.Versions)
                             .FirstOrDefaultAsync(ci => ci.Name == name, cancellationToken);
    }

    public async Task AddAsync(ClientInstance clientInstance, CancellationToken cancellationToken)
    {
        await _context.ClientInstances.AddAsync(clientInstance, cancellationToken);
        // SaveChangesAsync is called by IUnitOfWork
    }

    public Task UpdateAsync(ClientInstance clientInstance, CancellationToken cancellationToken)
    {
        // EF Core tracks changes to loaded entities. Explicitly calling Update marks the entire entity as modified.
        // If the entity is already tracked and only specific properties changed, this is often not necessary.
        // However, it's a safe call if the entity might be detached or if you want to ensure all properties are considered for update.
        _context.ClientInstances.Update(clientInstance);
        // SaveChangesAsync is called by IUnitOfWork
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<ClientInstance>> ListAllAsync(CancellationToken cancellationToken)
    {
        return await _context.ClientInstances
                             .Include(ci => ci.ClientConfiguration)
                                .ThenInclude(cc => cc!.Versions)
                             .AsNoTracking() // Good practice for read-only lists
                             .ToListAsync(cancellationToken);
    }
}