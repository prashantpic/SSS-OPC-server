using Microsoft.AspNetCore.SignalR;
using Services.Integration.Api.Hubs;
using Services.Integration.Application.Interfaces;

namespace Services.Integration.Infrastructure.Streaming;

/// <summary>
/// A concrete implementation of <see cref="IArDataStreamer"/> that uses the SignalR IHubContext
/// to send messages through the <see cref="ArDataStreamerHub"/>. This decouples application logic
/// from the hub implementation itself.
/// </summary>
public class ArDataStreamer : IArDataStreamer
{
    private readonly IHubContext<ArDataStreamerHub> _hubContext;
    private readonly ILogger<ArDataStreamer> _logger;
    private const string ClientMethodName = "ReceiveData";

    /// <summary>
    /// Initializes a new instance of the <see cref="ArDataStreamer"/> class.
    /// </summary>
    /// <param name="hubContext">The SignalR hub context for the AR streamer hub.</param>
    /// <param name="logger">The logger for structured logging.</param>
    public ArDataStreamer(IHubContext<ArDataStreamerHub> hubContext, ILogger<ArDataStreamer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StreamDataToAllAsync(object data)
    {
        _logger.LogDebug("Streaming data to all connected AR clients.");
        try
        {
            await _hubContext.Clients.All.SendAsync(ClientMethodName, data);
            _logger.LogInformation("Successfully streamed data to all AR clients.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while streaming data to all AR clients.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StreamDataToGroupAsync(string groupName, object data)
    {
        if (string.IsNullOrWhiteSpace(groupName))
        {
            _logger.LogWarning("StreamDataToGroupAsync called with a null or empty group name.");
            return;
        }

        _logger.LogDebug("Streaming data to AR client group '{GroupName}'.", groupName);
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync(ClientMethodName, data);
            _logger.LogInformation("Successfully streamed data to AR group '{GroupName}'.", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while streaming data to AR group '{GroupName}'.", groupName);
            throw;
        }
    }
}