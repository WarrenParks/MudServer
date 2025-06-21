using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MudServer.Server.Services;

public interface IChatManager
{
    public Task SendMessageAsync(string message, Guid toClientId, CancellationToken cancellationToken);
    public Task SendMessageAsync(string message, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken);
}

public class ChatManager(
  IConnectionManager connectionManager,
  ILogger<ChatManager> logger) : IChatManager
{
    private readonly IConnectionManager connectionManager = connectionManager;
    private readonly ILogger<ChatManager> logger = logger;

    public async Task SendMessageAsync(string message, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken)
    {
        // Implementation for sending a message to a specific client
        logger.LogInformation("Sending message: {message} from {fromClientId} to {toClientId}: ", message, fromClientId, toClientId);

        var webSocket = this.connectionManager.GetConnection(toClientId);

        var response = new { action = "chat", toClientId };
        var responseJson = JsonSerializer.Serialize(response);
        var responseBytes = Encoding.UTF8.GetBytes(responseJson);

        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, cancellationToken);
        // Here you would typically use the connection manager to get the WebSocket and send the message
    }

    public async Task SendMessageAsync(string message, Guid toClientId, CancellationToken cancellationToken)
    {
        await this.SendMessageAsync(message, Guid.Empty, toClientId, cancellationToken);
    }
}