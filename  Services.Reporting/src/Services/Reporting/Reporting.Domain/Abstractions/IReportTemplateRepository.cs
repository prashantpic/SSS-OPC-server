using Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate;

namespace Opc.System.Services.Reporting.Domain.Abstractions;

/// <summary>
/// Contract for a repository that manages the persistence of ReportTemplate aggregates.
/// To decouple the domain model from the specific data storage technology used for report templates.
/// </summary>
public interface IReportTemplateRepository
{
    /// <summary>
    /// Gets a report template by its unique identifier.
    /// </summary>
    /// <param name="id">The template ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The report template if found; otherwise, null.</returns>
    Task<ReportTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new report template to the repository.
    /// </summary>
    /// <param name="template">The report template to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(ReportTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report template in the repository.
    /// </summary>
    /// <param name="template">The report template to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(ReportTemplate template, CancellationToken cancellationToken = default);
}