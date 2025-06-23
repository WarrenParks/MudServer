
using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public class PingCommand(
    ILogger<PingCommand> logger,
    INotificationManager notificationManager) : IGameCommand
{
    private readonly ILogger<PingCommand> logger = logger;

    private readonly INotificationManager notificationManager = notificationManager;

    public string ActionType => "ping";

    public Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ping command executed by user {UserId}", context.ClientId);

        this.notificationManager.NotifyClient(context.ClientId, "Pong! from server to user");
        return Task.CompletedTask;
    }
}