using Microsoft.Extensions.Logging;
using SSS.Services.Notification.Application.Abstractions;
using SSS.Services.Notification.Application.DTOs;
using SSS.Services.Notification.Infrastructure.Channels.Abstractions;
using SSS.Services.Notification.Infrastructure.Templates.Abstractions;

namespace SSS.Services.Notification.Application.Services;

/// <summary>
/// Implements INotificationService to orchestrate template rendering and channel dispatching.
/// This is the core orchestration service that receives a notification request, resolves
/// the appropriate channels and templates, and manages the dispatch process.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEnumerable<INotificationChannel> _channels;
    private readonly ITemplateService _templateService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationChannel> channels,
        ITemplateService templateService,
        ILogger<NotificationService> logger)
    {
        _channels = channels;
        _templateService = templateService;
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<bool> SendNotificationAsync(NotificationRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting notification process for TemplateId: {TemplateId} to {RecipientCount} recipients.", 
            request.TemplateId, request.Recipients.Count);

        var anySentSuccessfully = false;

        foreach (var channelType in request.TargetChannels)
        {
            var channel = _channels.FirstOrDefault(c => c.ChannelType == channelType);

            if (channel is null)
            {
                _logger.LogWarning("No notification channel implementation found for type: {ChannelType}. Skipping.", channelType);
                continue;
            }

            try
            {
                var (subject, body) = await _templateService.RenderTemplateAsync(
                    channelType, 
                    request.TemplateId, 
                    request.TemplateData, 
                    cancellationToken);

                if (string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogError("Template rendering for {TemplateId} on channel {ChannelType} resulted in an empty body. Skipping channel.",
                        request.TemplateId, channelType);
                    continue;
                }

                foreach (var recipient in request.Recipients)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await channel.SendAsync(recipient, subject, body, cancellationToken);
                        _logger.LogInformation("Successfully dispatched notification for recipient via {ChannelType}.", channelType);
                        anySentSuccessfully = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification to recipient via {ChannelType}. Continuing to next recipient/channel.", channelType);
                        // Do not re-throw; allow other notifications to proceed.
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while processing channel {ChannelType}. Continuing to next channel.", channelType);
            }
        }
        
        _logger.LogInformation("Notification process finished for TemplateId: {TemplateId}. Overall success: {Success}",
            request.TemplateId, anySentSuccessfully);
            
        return anySentSuccessfully;
    }
}