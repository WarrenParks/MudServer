using System.Net.WebSockets;

using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface INotificationManager
{
    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    Task NotifyAllAsync(string message, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a notification to a specific client.
    /// </summary>
    /// <param name="clientId">The ID of the client to notify.</param>
    /// <param name="message">The message to send.</param>
    Task NotifyClientAsync(Guid clientId, string message, CancellationToken cancellationToken);
}

public class NotificationManager(
    IConnectionManager connectionManager,
    ILogger<NotificationManager> logger,
    IWebSocketMessenger webSocketMessenger) : INotificationManager
{
    private readonly IConnectionManager connectionManager = connectionManager;
    private readonly ILogger<NotificationManager> logger = logger;
    private readonly IWebSocketMessenger webSocketMessenger = webSocketMessenger;

    public async Task NotifyAllAsync(string message, CancellationToken cancellationToken)
    {
        await this.webSocketMessenger.SendMessageAsync(Actions.Notification, message, Guid.Empty, cancellationToken);
    }

    public async Task NotifyClientAsync(Guid clientId, string message, CancellationToken cancellationToken)
    {
        await this.webSocketMessenger.SendMessageAsync(Actions.Notification, message, Guid.Empty, clientId, cancellationToken);
    }
}