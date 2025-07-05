using Microsoft.EntityFrameworkCore;
using Reporting.Application.Contracts.Infrastructure;
using Reporting.Domain.Aggregates;

namespace Reporting.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of the IReportTemplateRepository interface.
/// Handles database operations for ReportTemplate aggregates.
/// </summary>
public class ReportTemplateRepository : IReportTemplateRepository
{
    private readonly ReportingDbContext _dbContext;

    public ReportTemplateRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FindAsync is efficient for finding by primary key.
        // EF Core automatically handles loading of owned types (Branding, Schedule).
        return await _dbContext.ReportTemplates.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<ReportTemplate>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReportTemplates.ToListAsync(cancellationToken);
    }

    public async Task<ReportTemplate> AddAsync(ReportTemplate entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.ReportTemplates.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(ReportTemplate entity, CancellationToken cancellationToken = default)
    {
        // Mark the entire entity as modified. EF Core will figure out what changed.
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ReportTemplate entity, CancellationToken cancellationToken = default)
    {
        _dbContext.ReportTemplates.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}