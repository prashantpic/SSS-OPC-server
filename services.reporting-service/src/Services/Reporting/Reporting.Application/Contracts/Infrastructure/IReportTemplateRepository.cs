using Reporting.Domain.Aggregates;

namespace Reporting.Application.Contracts.Infrastructure;

/// <summary>
/// Defines the contract for data access operations related to the ReportTemplate aggregate.
/// </summary>
public interface IReportTemplateRepository
{
    /// <summary>
    /// Gets a report template by its unique identifier.
    /// </summary>
    Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all report templates.
    /// </summary>
    Task<IReadOnlyList<ReportTemplate>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new report template to the data store.
    /// </summary>
    Task<ReportTemplate> AddAsync(ReportTemplate entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report template in the data store.
    /// </summary>
    Task UpdateAsync(ReportTemplate entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a report template from the data store.
    /// </summary>
    Task DeleteAsync(ReportTemplate entity, CancellationToken cancellationToken = default);
}