using Microsoft.AspNetCore.SignalR;

namespace Services.Integration.Api.Hubs;

/// <summary>
/// Implements the server-side SignalR Hub to manage WebSocket connections with Augmented Reality clients
/// and provide a real-time data streaming channel.
/// </summary>
public class ArDataStreamerHub : Hub
{
    private readonly ILogger<ArDataStreamerHub> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArDataStreamerHub"/> class.
    /// </summary>
    /// <param name="logger">The logger for structured logging.</param>
    public ArDataStreamerHub(ILogger<ArDataStreamerHub> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Called when a new connection is established with the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("AR client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a connection with the hub is terminated.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "AR client disconnected with error: {ConnectionId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("AR client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows a connected client to join a specific group for targeted data streams.
    /// </summary>
    /// <param name="groupName">The name of the group to join.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} joined AR group '{GroupName}'", Context.ConnectionId, groupName);
    }

    /// <summary>
    /// Allows a connected client to leave a specific group.
    /// </summary>
    /// <param name="groupName">The name of the group to leave.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation("Client {ConnectionId} left AR group '{GroupName}'", Context.ConnectionId, groupName);
    }
}