using Reporting.Domain.Aggregates;

namespace Reporting.Application.Contracts.Infrastructure;

/// <summary>
/// Defines the contract for data access operations related to the GeneratedReport aggregate.
/// </summary>
public interface IGeneratedReportRepository
{
    /// <summary>
    /// Gets a generated report by its unique identifier.
    /// </summary>
    Task<GeneratedReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new generated report to the data store.
    /// </summary>
    Task<GeneratedReport> AddAsync(GeneratedReport entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing generated report in the data store.
    /// </summary>
    Task UpdateAsync(GeneratedReport entity, CancellationToken cancellationToken = default);
}