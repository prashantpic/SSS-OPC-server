using Microsoft.EntityFrameworkCore;
using Reporting.Application.Contracts.Infrastructure;
using Reporting.Domain.Aggregates;

namespace Reporting.Infrastructure.Persistence.Repositories;

public class GeneratedReportRepository : IGeneratedReportRepository
{
    private readonly ReportingDbContext _dbContext;

    public GeneratedReportRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<GeneratedReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GeneratedReports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<GeneratedReport> AddAsync(GeneratedReport entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.GeneratedReports.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(GeneratedReport entity, CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}