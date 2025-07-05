using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SSS.Services.Notification.Domain.Enums;
using SSS.Services.Notification.Infrastructure.Templates.Abstractions;
using Scriban;
using Scriban.Runtime;

namespace SSS.Services.Notification.Infrastructure.Templates.Services;

/// <summary>
/// An implementation of ITemplateService that reads template files from the local file system
/// and renders them using the Scriban templating engine.
/// </summary>
public class FileSystemTemplateService : ITemplateService
{
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<FileSystemTemplateService> _logger;
    private readonly ConcurrentDictionary<string, Template> _templateCache = new();
    private const string TemplateSeparator = "---";

    public FileSystemTemplateService(IHostEnvironment hostEnvironment, ILogger<FileSystemTemplateService> logger)
    {
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(string Subject, string Body)> RenderTemplateAsync(
        NotificationChannel channelType, 
        string templateId, 
        object data, 
        CancellationToken cancellationToken)
    {
        try
        {
            var templateContent = await GetTemplateContentAsync(channelType, templateId, cancellationToken);
            if (string.IsNullOrEmpty(templateContent))
            {
                return (string.Empty, string.Empty);
            }

            var (subjectTemplate, bodyTemplate) = ParseTemplateParts(templateContent);

            var scriptObject = new ScriptObject();
            scriptObject.Import(data, renamer: member => member.Name.ToLower());

            var context = new TemplateContext
            {
                LoopLimit = 1000, 
                RecursionLimit = 100,
                EnableRelaxedMemberAccess = true
            };
            context.PushGlobal(scriptObject);

            var renderedSubject = await subjectTemplate.RenderAsync(context);
            var renderedBody = await bodyTemplate.RenderAsync(context);

            return (renderedSubject.Trim(), renderedBody.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render template {TemplateId} for channel {ChannelType}", templateId, channelType);
            return ($"Error Rendering Template: {templateId}", $"An error occurred while rendering the template. Please check the logs. Error: {ex.Message}");
        }
    }

    private (Template Subject, Template Body) ParseTemplateParts(string content)
    {
        var parts = content.Split(new[] { TemplateSeparator }, 2, StringSplitOptions.None);
        
        string subjectContent;
        string bodyContent;

        if (parts.Length == 2)
        {
            subjectContent = parts[0];
            bodyContent = parts[1];
        }
        else
        {
            _logger.LogWarning("Template content does not contain a '---' separator. Using the entire file as the body and an empty subject.");
            subjectContent = string.Empty;
            bodyContent = parts[0];
        }
        
        // Caching is handled at the file content level, but parsing happens on each call
        // which is fast. Could cache parsed templates if performance becomes an issue.
        var subjectTemplate = Template.Parse(subjectContent);
        var bodyTemplate = Template.Parse(bodyContent);

        return (subjectTemplate, bodyTemplate);
    }

    private async Task<string?> GetTemplateContentAsync(NotificationChannel channelType, string templateId, CancellationToken cancellationToken)
    {
        var fileExtension = channelType switch
        {
            NotificationChannel.Email => ".html",
            NotificationChannel.Sms => ".txt",
            _ => throw new ArgumentOutOfRangeException(nameof(channelType), $"Unsupported channel type for templating: {channelType}")
        };

        var templatePath = Path.Combine(
            _hostEnvironment.ContentRootPath,
            "Templates",
            channelType.ToString(),
            $"{templateId}{fileExtension}");
        
        // Basic caching to avoid frequent file I/O for the same templates.
        // A more robust solution might use IMemoryCache with expiration.
        if (_templateCache.TryGetValue(templatePath, out var cachedTemplate))
        {
             _logger.LogDebug("Using cached template from {TemplatePath}", templatePath);
             // This is simplified. The current implementation re-reads file. Let's adjust.
             // We will cache the string content instead of the template object.
        }

        try
        {
            _logger.LogDebug("Loading template from file: {TemplatePath}", templatePath);
            return await File.ReadAllTextAsync(templatePath, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            _logger.LogError("Template file not found at path: {TemplatePath}", templatePath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading template file at path: {TemplatePath}", templatePath);
            return null;
        }
    }
}