using MediatR;
using Reporting.Application.Contracts.Generation;
using Reporting.Application.Contracts.Infrastructure;
using Reporting.Application.Contracts.Services;
using Reporting.Application.Models;
using Reporting.Domain.Aggregates;
using Reporting.Domain.Enums;

namespace Reporting.Application.Generation.Commands.GenerateReportOnDemand;

/// <summary>
/// The handler for the GenerateReportOnDemandCommand. Orchestrates the entire report generation process.
/// </summary>
public class GenerateReportOnDemandCommandHandler : IRequestHandler<GenerateReportOnDemandCommand, Guid>
{
    private readonly IReportTemplateRepository _reportTemplateRepository;
    private readonly IGeneratedReportRepository _generatedReportRepository;
    private readonly IDataServiceClient _dataServiceClient;
    private readonly IAiServiceClient _aiServiceClient;
    private readonly IReportGeneratorFactory _reportGeneratorFactory;
    private readonly IFileStorageService _fileStorageService;

    public GenerateReportOnDemandCommandHandler(
        IReportTemplateRepository reportTemplateRepository,
        IGeneratedReportRepository generatedReportRepository,
        IDataServiceClient dataServiceClient,
        IAiServiceClient aiServiceClient,
        IReportGeneratorFactory reportGeneratorFactory,
        IFileStorageService fileStorageService)
    {
        _reportTemplateRepository = reportTemplateRepository;
        _generatedReportRepository = generatedReportRepository;
        _dataServiceClient = dataServiceClient;
        _aiServiceClient = aiServiceClient;
        _reportGeneratorFactory = reportGeneratorFactory;
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Coordinates fetching data, generating the report document, and saving it.
    /// </summary>
    public async Task<Guid> Handle(GenerateReportOnDemandCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the template
        var template = await _reportTemplateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new ApplicationException($"ReportTemplate with ID '{request.TemplateId}' not found.");

        var outputFormat = Enum.Parse<ReportFormat>(request.OutputFormat, true);
        
        // 2. Create a 'job' record for the report
        var generatedReport = GeneratedReport.Create(Guid.NewGuid(), template.Id, outputFormat);
        await _generatedReportRepository.AddAsync(generatedReport, cancellationToken);

        try
        {
            // 3. Collect data from various services
            var reportDataModel = await CollectReportData(template, cancellationToken);

            // 4. Get the correct generator for the requested format
            var generator = _reportGeneratorFactory.Create(outputFormat);

            // 5. Generate the report file content
            byte[] reportBytes = await generator.GenerateAsync(reportDataModel, cancellationToken);
            
            // 6. Save the file using the storage service
            var fileName = $"{template.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMddHHmmss}.{outputFormat.ToString().ToLower()}";
            var filePath = await _fileStorageService.SaveAsync(reportBytes, fileName, "reports", cancellationToken);

            // 7. Update the job record to 'Completed'
            generatedReport.MarkAsCompleted(filePath);
            await _generatedReportRepository.UpdateAsync(generatedReport, cancellationToken);
        }
        catch (Exception ex)
        {
            // 8. If anything fails, update the job record to 'Failed'
            generatedReport.MarkAsFailed(ex.Message);
            await _generatedReportRepository.UpdateAsync(generatedReport, cancellationToken);
            // Optionally re-throw or handle the exception as needed
            throw;
        }

        // 9. Return the ID of the GeneratedReport 'job'
        return generatedReport.Id;
    }

    private async Task<ReportDataModel> CollectReportData(ReportTemplate template, CancellationToken cancellationToken)
    {
        var model = new ReportDataModel
        {
            ReportTitle = template.Name,
            GeneratedAt = DateTime.UtcNow
        };

        // This is a simplified example. A real implementation would be more robust.
        var now = DateTime.UtcNow;
        var startTime = now.AddDays(-1);
        var endTime = now;

        foreach (var dataSource in template.DataSources)
        {
            switch (dataSource.Type.ToLowerInvariant())
            {
                case "historicaldata":
                    var tag = dataSource.Parameters.GetValueOrDefault("tag");
                    if(tag is not null)
                    {
                       var data = await _dataServiceClient.GetHistoricalDataAsync(tag, startTime, endTime, cancellationToken);
                       model.DataSections.Add(new { Title = dataSource.Name, Data = data });
                    }
                    break;
                case "aianomalies":
                    var anomalies = await _aiServiceClient.GetAnomaliesForReportAsync(startTime, endTime, cancellationToken);
                    model.DataSections.Add(new { Title = dataSource.Name, Data = anomalies });
                    break;
                // Add more cases here for other data source types
            }
        }

        return model;
    }
}