
using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public class PingCommand(
    ILogger<PingCommand> logger,
    IChatManager chatManager) : IGameCommand
{
    private readonly ILogger<PingCommand> logger = logger;
    private readonly IChatManager chatManager = chatManager;

    public string ActionType => "ping";

    public string Description => "Checks if the server is responsive.";

    public async Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        logger.LogInformation("Ping command executed by user {UserId}", context.ClientId);

        await this.chatManager.SendMessageAsync("Pong! from server to user", context.ClientId, cancellationToken);
    }
}