using System.Net.WebSockets;

namespace MudServer.Server.Services;

public interface INotificationManager
{
    /// <summary>
    /// Sends a notification to all connected clients.
    /// </summary>
    /// <param name="message">The message to send.</param>
    Task NotifyAll(string message);

    /// <summary>
    /// Sends a notification to a specific client.
    /// </summary>
    /// <param name="clientId">The ID of the client to notify.</param>
    /// <param name="message">The message to send.</param>
    Task NotifyClient(Guid clientId, string message);
}

public class NotificationManager(IConnectionManager connectionManager) : INotificationManager
{
    private readonly IConnectionManager connectionManager = connectionManager;

    public async Task NotifyAll(string message)
    {
        var tasks = this.connectionManager.GetAllConnections()
            .Select(client => this.NotifyClient(client.Key, message));
        await Task.WhenAll(tasks);
    }

    public async Task NotifyClient(Guid clientId, string message)
    {
        var webSocket = this.connectionManager.GetConnection(clientId);
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}