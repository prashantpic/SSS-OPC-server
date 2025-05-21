using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Domain.Aggregates.ConfigurationMigrationJobAggregate;
using ManagementService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Infrastructure.Persistence.Repositories;

public class ConfigurationMigrationJobRepository : IConfigurationMigrationJobRepository
{
    private readonly ManagementDbContext _context;

    public ConfigurationMigrationJobRepository(ManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ConfigurationMigrationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ConfigurationMigrationJobs
                             .Include(job => job.ValidationMessages) // Eager load validation messages
                             .FirstOrDefaultAsync(job => job.Id == id, cancellationToken);
    }

    public async Task AddAsync(ConfigurationMigrationJob job, CancellationToken cancellationToken)
    {
        await _context.ConfigurationMigrationJobs.AddAsync(job, cancellationToken);
    }

    public Task UpdateAsync(ConfigurationMigrationJob job, CancellationToken cancellationToken)
    {
        _context.ConfigurationMigrationJobs.Update(job);
        return Task.CompletedTask;
    }
}