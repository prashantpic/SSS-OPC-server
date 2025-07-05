using MediatR;
using Microsoft.Extensions.Logging;
using Opc.System.Services.Reporting.Application.Abstractions;
using Opc.System.Services.Reporting.Application.Models;
using Opc.System.Services.Reporting.Domain.Abstractions;
using Opc.System.Services.Reporting.Domain.Aggregates.GeneratedReport;

namespace Opc.System.Services.Reporting.Application.Features.Reports.Commands.GenerateReport;

// Assuming GenerateReportCommand is defined elsewhere in this namespace
public record GenerateReportCommand(Guid ReportTemplateId, ReportFormat Format) : IRequest<Guid>;

// Assuming ReportFormat enum is defined in the Application layer
public enum ReportFormat { Pdf, Excel, Html }

// Assuming an event for distribution is defined
public record ReportGeneratedEvent(Guid GeneratedReportId) : INotification;

/// <summary>
/// Orchestrates the report generation use case by fetching template, data, and insights, 
/// then invoking the generation engine and persisting the result.
/// Contains the core application logic for orchestrating the generation of a single report.
/// </summary>
public class GenerateReportCommandHandler : IRequestHandler<GenerateReportCommand, Guid>
{
    private readonly ILogger<GenerateReportCommandHandler> _logger;
    private readonly IReportTemplateRepository _reportTemplateRepository;
    private readonly IGeneratedReportRepository _generatedReportRepository;
    private readonly IDataServiceClient _dataServiceClient;
    private readonly IAiServiceClient _aiServiceClient;
    private readonly IReportGenerationEngine _reportGenerationEngine;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPublisher _publisher;

    public GenerateReportCommandHandler(
        ILogger<GenerateReportCommandHandler> logger,
        IReportTemplateRepository reportTemplateRepository,
        IGeneratedReportRepository generatedReportRepository,
        IDataServiceClient dataServiceClient,
        IAiServiceClient aiServiceClient,
        IReportGenerationEngine reportGenerationEngine,
        IFileStorageService fileStorageService,
        IPublisher publisher)
    {
        _logger = logger;
        _reportTemplateRepository = reportTemplateRepository;
        _generatedReportRepository = generatedReportRepository;
        _dataServiceClient = dataServiceClient;
        _aiServiceClient = aiServiceClient;
        _reportGenerationEngine = reportGenerationEngine;
        _fileStorageService = fileStorageService;
        _publisher = publisher;
    }

    /// <summary>
    /// Handles the GenerateReportCommand. Orchestrates the process of fetching data, 
    /// calling the generation engine, and saving the resulting report.
    /// </summary>
    /// <param name="request">The command containing the template ID and desired format.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created GeneratedReport instance.</returns>
    public async Task<Guid> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting report generation for template {TemplateId}", request.ReportTemplateId);

        var reportTemplate = await _reportTemplateRepository.GetByIdAsync(request.ReportTemplateId, cancellationToken);
        if (reportTemplate is null)
        {
            _logger.LogWarning("Report template {TemplateId} not found.", request.ReportTemplateId);
            throw new ArgumentException($"Report template with ID {request.ReportTemplateId} not found.");
        }

        // 1. Create a new GeneratedReport entity and save it with Pending status.
        var nextVersion = await _generatedReportRepository.GetNextVersionAsync(reportTemplate.Id, cancellationToken);
        var generatedReport = GeneratedReport.Create(reportTemplate.Id, nextVersion);
        await _generatedReportRepository.AddAsync(generatedReport, cancellationToken);

        try
        {
            generatedReport.StartGeneration();
            await _generatedReportRepository.UpdateAsync(generatedReport, cancellationToken);

            // 2. Fetch data from various sources
            var reportDataModel = await GatherReportData(reportTemplate, cancellationToken);

            // 3. Invoke the report generation engine
            await using var reportStream = await _reportGenerationEngine.GenerateAsync(reportDataModel, request.Format, cancellationToken);

            // 4. Save the file using the storage service
            var fileName = $"{reportTemplate.Name.Replace(" ", "_")}_{generatedReport.Id}.{request.Format.ToString().ToLower()}";
            var storagePath = await _fileStorageService.SaveFileAsync(fileName, reportStream, cancellationToken);
            
            // 5. Update the GeneratedReport entity
            generatedReport.MarkAsCompleted(storagePath);
            await _generatedReportRepository.UpdateAsync(generatedReport, cancellationToken);

            _logger.LogInformation("Successfully generated report {ReportId} for template {TemplateId}. Stored at: {StoragePath}",
                generatedReport.Id, reportTemplate.Id, storagePath);
            
            // 6. Dispatch a notification for distribution
            await _publisher.Publish(new ReportGeneratedEvent(generatedReport.Id), cancellationToken);

            return generatedReport.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate report for template {TemplateId}", request.ReportTemplateId);
            generatedReport.MarkAsFailed(ex.Message);
            await _generatedReportRepository.UpdateAsync(generatedReport, cancellationToken);
            throw; // Re-throw to indicate failure to the caller
        }
    }
    
    private async Task<ReportDataModel> GatherReportData(Domain.Aggregates.ReportTemplate.ReportTemplate template, CancellationToken cancellationToken)
    {
        var dataPoints = new List<DataPoint>();
        var anomalyInsights = new List<AnomalyInsight>();

        // This is a simplified example. A real implementation would be more robust.
        var timeRange = new TimeRange(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow);

        foreach (var dataSource in template.DataSources)
        {
            if (dataSource.SourceType == "Historical" && dataSource.Parameters.TryGetValue("tags", out var tags))
            {
                var query = new HistoricalDataQuery(tags.Split(','), timeRange.StartTime, timeRange.EndTime, "avg");
                var historicalData = await _dataServiceClient.GetHistoricalDataAsync(query, cancellationToken);
                dataPoints.AddRange(historicalData);
            }
            else if (dataSource.SourceType == "AIInsight")
            {
                var anomalies = await _aiServiceClient.GetAnomaliesAsync(timeRange, cancellationToken);
                anomalyInsights.AddRange(anomalies);
            }
        }
        
        return new ReportDataModel(template.Name, timeRange, dataPoints, anomalyInsights);
    }
}