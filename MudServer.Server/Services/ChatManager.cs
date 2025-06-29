using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface IChatManager
{
    public Task SendMessageAsync(string message, Guid fromClientId, CancellationToken cancellationToken);
    public Task SendMessageAsync(string message, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken);
}

public class ChatManager(
  IWebSocketMessenger webSocketMessenger,
  IConnectionManager connectionManager) : IChatManager
{
    private readonly IWebSocketMessenger webSocketMessenger = webSocketMessenger;
    private readonly IConnectionManager connectionManager = connectionManager;

    public async Task SendMessageAsync(string message, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken)
    {
        var fromUser = this.connectionManager.GetUser(fromClientId);

        await this.webSocketMessenger.SendMessageAsync(
            Actions.Chat, new { fromUser = fromUser?.Name, message }, fromClientId, toClientId, cancellationToken);
    }

    public async Task SendMessageAsync(string message, Guid fromClientId, CancellationToken cancellationToken)
    {
        var fromUser = this.connectionManager.GetUser(fromClientId);

        await this.webSocketMessenger.SendMessageAsync(
            Actions.Chat, new { fromUser = fromUser?.Name, message }, fromClientId, cancellationToken);
    }
}