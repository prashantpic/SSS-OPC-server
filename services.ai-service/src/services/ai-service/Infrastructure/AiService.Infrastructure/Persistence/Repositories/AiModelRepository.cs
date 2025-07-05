using AiService.Domain.Aggregates.AiModel;
using AiService.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AiService.Infrastructure.Persistence.Repositories;

public class AiModelRepository : IAiModelRepository
{
    private readonly AiServiceDbContext _context;

    public AiModelRepository(AiServiceDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AiModel model, CancellationToken cancellationToken = default)
    {
        await _context.AiModels.AddAsync(model, cancellationToken);
    }

    public async Task<AiModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AiModels
            .Include(m => m.Artifacts)
            .Include(m => m.PerformanceHistory)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public Task UpdateAsync(AiModel model, CancellationToken cancellationToken = default)
    {
        // In EF Core, just modifying the tracked entity is enough. 
        // The SaveChanges call in the Unit of Work will handle the update.
        _context.Entry(model).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}