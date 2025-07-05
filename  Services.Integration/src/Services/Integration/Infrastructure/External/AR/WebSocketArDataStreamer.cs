using Microsoft.Extensions.Logging;
using Opc.System.Services.Integration.Application.Contracts.External;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Opc.System.Services.Integration.Infrastructure.External.AR;

/// <summary>
/// Singleton service implementation of IArDataStreamer using WebSockets.
/// Manages WebSocket connections and streams data to connected AR clients.
/// </summary>
public class WebSocketArDataStreamer : IArDataStreamer
{
    private readonly ILogger<WebSocketArDataStreamer> _logger;
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

    public WebSocketArDataStreamer(ILogger<WebSocketArDataStreamer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a new WebSocket connection for a given device.
    /// </summary>
    /// <param name="deviceId">The unique identifier for the device.</param>
    /// <param name="socket">The WebSocket instance.</param>
    public void AddSocket(string deviceId, WebSocket socket)
    {
        if (_sockets.TryAdd(deviceId, socket))
        {
            _logger.LogInformation("AR device connected: {DeviceId}. Total devices: {DeviceCount}", deviceId, _sockets.Count);
        }
        else
        {
            _logger.LogWarning("Failed to add AR device {DeviceId}. A device with this ID may already be connected.", deviceId);
        }
    }

    /// <summary>
    /// Removes a WebSocket connection for a given device.
    /// </summary>
    /// <param name="deviceId">The unique identifier for the device.</param>
    public async Task RemoveSocket(string deviceId)
    {
        if (_sockets.TryRemove(deviceId, out var socket))
        {
            try
            {
                if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by server.", CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while closing socket for device {DeviceId}.", deviceId);
            }
            finally
            {
                socket.Dispose();
            }
            _logger.LogInformation("AR device disconnected: {DeviceId}. Total devices: {DeviceCount}", deviceId, _sockets.Count);
        }
    }

    /// <inheritdoc />
    public int GetConnectedDeviceCount() => _sockets.Count;

    /// <inheritdoc />
    public async Task StreamDataToAllAsync(string payload)
    {
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var tasks = _sockets.ToList().Select(pair => StreamToSocket(pair.Key, pair.Value, payloadBytes));
        await Task.WhenAll(tasks);
    }

    /// <inheritdoc />
    public async Task StreamDataToDeviceAsync(string deviceId, string payload)
    {
        if (_sockets.TryGetValue(deviceId, out var socket))
        {
            var payloadBytes = Encoding.UTF8.GetBytes(payload);
            await StreamToSocket(deviceId, socket, payloadBytes);
        }
        else
        {
            _logger.LogTrace("Attempted to stream to non-existent device: {DeviceId}", deviceId);
        }
    }

    private async Task StreamToSocket(string deviceId, WebSocket socket, byte[] payloadBytes)
    {
        try
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(new ArraySegment<byte>(payloadBytes, 0, payloadBytes.Length), 
                                       WebSocketMessageType.Text, 
                                       true, 
                                       CancellationToken.None);
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "WebSocketException while streaming to device {DeviceId}. Removing socket.", deviceId);
            await RemoveSocket(deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while streaming to device {DeviceId}. Removing socket.", deviceId);
            await RemoveSocket(deviceId);
        }
    }
}