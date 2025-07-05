using Microsoft.Extensions.Logging;
using Opc.System.Services.Reporting.Application.Abstractions;
using Opc.System.Services.Reporting.Domain.Abstractions;
using Opc.System.Services.Reporting.Domain.Aggregates.ReportTemplate.ValueObjects;

namespace Opc.System.Services.Reporting.Application.Features.Distribution;

/// <summary>
/// Defines the contract for a service that distributes generated reports.
/// </summary>
public interface IReportDistributionService
{
    /// <summary>
    /// Distributes a report to its configured targets.
    /// </summary>
    /// <param name="generatedReportId">The ID of the report to distribute.</param>
    /// <param name="targets">The collection of distribution targets.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DistributeAsync(Guid generatedReportId, IEnumerable<DistributionTarget> targets, CancellationToken cancellationToken);
}

/// <summary>
/// Handles the business logic for distributing a generated report to various channels like email or file shares based on configured targets.
/// To manage the multi-channel distribution of a completed report.
/// </summary>
public class ReportDistributionService : IReportDistributionService
{
    private readonly ILogger<ReportDistributionService> _logger;
    private readonly IEmailService _emailService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IGeneratedReportRepository _reportRepository;

    public ReportDistributionService(
        ILogger<ReportDistributionService> logger,
        IEmailService emailService,
        IFileStorageService fileStorageService,
        IGeneratedReportRepository reportRepository)
    {
        _logger = logger;
        _emailService = emailService;
        _fileStorageService = fileStorageService;
        _reportRepository = reportRepository;
    }

    /// <inheritdoc />
    public async Task DistributeAsync(Guid generatedReportId, IEnumerable<DistributionTarget> targets, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting distribution for report {ReportId}", generatedReportId);

        var report = await _reportRepository.GetByIdAsync(generatedReportId, cancellationToken);
        if (report?.StoragePath is null)
        {
            _logger.LogError("Cannot distribute report {ReportId}. Report not found or has no storage path.", generatedReportId);
            return;
        }

        try
        {
            await using var fileStream = await _fileStorageService.GetFileStreamAsync(report.StoragePath, cancellationToken);
            var fileName = Path.GetFileName(report.StoragePath);

            foreach (var target in targets)
            {
                // Reset stream position for each recipient
                fileStream.Position = 0;

                switch (target.Channel)
                {
                    case DistributionChannel.Email:
                        _logger.LogInformation("Distributing report {ReportId} via Email to {Address}", generatedReportId, target.Address);
                        await _emailService.SendEmailWithAttachmentAsync(
                            target.Address,
                            $"Report: {fileName}",
                            "Please find the attached report.",
                            fileStream,
                            fileName,
                            cancellationToken);
                        break;
                    
                    case DistributionChannel.NetworkShare:
                        // In a real scenario, another service would handle this.
                        _logger.LogWarning("NetworkShare distribution channel is not implemented.");
                        break;

                    default:
                        _logger.LogWarning("Unknown distribution channel '{Channel}' for report {ReportId}", target.Channel, generatedReportId);
                        break;
                }
            }
            _logger.LogInformation("Finished distribution for report {ReportId}", generatedReportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during distribution of report {ReportId}", generatedReportId);
        }
    }
}