using MediatR;
using Reporting.Application.Contracts.Infrastructure;
using Reporting.Domain.Aggregates;
using Reporting.Domain.Enums;
using Reporting.Domain.ValueObjects;

namespace Reporting.Application.Templates.Commands.CreateReportTemplate;

/// <summary>
/// The handler for the CreateReportTemplateCommand. Contains the business logic to process the creation request.
/// </summary>
public class CreateReportTemplateCommandHandler : IRequestHandler<CreateReportTemplateCommand, Guid>
{
    private readonly IReportTemplateRepository _reportTemplateRepository;
    // In a real system, you would have a contract for IBackgroundJobScheduler
    // and Hangfire would be an implementation detail in the Infrastructure layer.
    // For simplicity here, we'll assume a contract that looks like Hangfire's client.
    private readonly IBackgroundJobClient _backgroundJobClient;

    public CreateReportTemplateCommandHandler(IReportTemplateRepository reportTemplateRepository, IBackgroundJobClient backgroundJobClient)
    {
        _reportTemplateRepository = reportTemplateRepository ?? throw new ArgumentNullException(nameof(reportTemplateRepository));
        _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
    }

    /// <summary>
    /// Orchestrates the creation of a ReportTemplate aggregate and persists it.
    /// </summary>
    public async Task<Guid> Handle(CreateReportTemplateCommand request, CancellationToken cancellationToken)
    {
        // 1. Convert DTOs to Domain Value Objects
        var branding = new Branding(request.Branding.LogoUrl, request.Branding.PrimaryColor, request.Branding.CompanyName);
        
        Schedule? schedule = null;
        if (!string.IsNullOrWhiteSpace(request.Schedule))
        {
            schedule = new Schedule(request.Schedule);
        }

        var dataSources = request.DataSources
            .Select(ds => new DataSource(ds.Name, ds.Type, ds.Parameters))
            .ToList();

        var format = Enum.Parse<ReportFormat>(request.DefaultFormat, true);
        
        // 2. Use the aggregate factory to create the domain entity
        var newTemplate = ReportTemplate.Create(
            Guid.NewGuid(),
            request.Name,
            branding,
            format,
            dataSources,
            schedule);

        // 3. Persist the new aggregate
        await _reportTemplateRepository.AddAsync(newTemplate, cancellationToken);
        
        // 4. Schedule the recurring job if a schedule is provided
        if (newTemplate.Schedule != null)
        {
            // The job ID is tied to the template ID to allow for updates/deletions.
            // A more complex job class would be in infrastructure, but this demonstrates the call.
            _backgroundJobClient.AddOrUpdate(
                recurringJobId: newTemplate.Id.ToString(),
                methodCall: () => Console.WriteLine($"GENERATE REPORT: TemplateId={newTemplate.Id}, Format={newTemplate.DefaultFormat}"), // This should call a job class in infra
                cronExpression: newTemplate.Schedule.CronExpression
            );
        }

        // 5. Return the ID of the new template
        return newTemplate.Id;
    }
}

// Dummy interface to satisfy the handler's dependency contract.
// The real one would be provided by a library like Hangfire.
public interface IBackgroundJobClient
{
    void AddOrUpdate(string recurringJobId, System.Linq.Expressions.Expression<Action> methodCall, string cronExpression);
}