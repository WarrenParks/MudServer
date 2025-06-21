namespace MudServer.Server.Commands;

public interface IGameCommand
{
    string ActionType { get; }

    Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken);
}