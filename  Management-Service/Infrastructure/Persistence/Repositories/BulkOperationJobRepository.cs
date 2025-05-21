using ManagementService.Application.Abstractions.Jobs;
using ManagementService.Domain.Aggregates.BulkOperationJobAggregate;
using ManagementService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagementService.Infrastructure.Persistence.Repositories;

public class BulkOperationJobRepository : IBulkOperationJobRepository
{
    private readonly ManagementDbContext _context;

    public BulkOperationJobRepository(ManagementDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<BulkOperationJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.BulkOperationJobs
                             .Include(job => job.ProgressDetails) // Eager load task details
                             .FirstOrDefaultAsync(job => job.Id == id, cancellationToken);
    }

    public async Task AddAsync(BulkOperationJob job, CancellationToken cancellationToken)
    {
        await _context.BulkOperationJobs.AddAsync(job, cancellationToken);
    }

    public Task UpdateAsync(BulkOperationJob job, CancellationToken cancellationToken)
    {
        _context.BulkOperationJobs.Update(job);
        return Task.CompletedTask;
    }
}