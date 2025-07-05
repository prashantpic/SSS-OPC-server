using LicensingService.Application.Contracts.Persistence;
using LicensingService.Domain.Aggregates;
using LicensingService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService.Infrastructure.Persistence.Repositories;

/// <summary>
/// The concrete implementation of the license repository using Entity Framework Core to interact with the database.
/// Provides a concrete implementation for persisting and retrieving License aggregates to/from a relational database.
/// </summary>
public class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public LicenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task AddAsync(License license, CancellationToken cancellationToken)
    {
        await _context.Licenses.AddAsync(license, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<License?> GetByKeyAsync(LicenseKey licenseKey, CancellationToken cancellationToken)
    {
        // Query based on the Value property of the LicenseKey value object.
        // Include related entities that are part of the aggregate.
        return await _context.Licenses
            .Include(l => l.Features) 
            .FirstOrDefaultAsync(l => l.Key == licenseKey, cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(License license, CancellationToken cancellationToken)
    {
        // EF Core's change tracker automatically handles updates when SaveChanges is called.
        // This method can be left empty or can explicitly set the entity state if needed.
        _context.Entry(license).State = EntityState.Modified;
        return Task.CompletedTask;
    }
}