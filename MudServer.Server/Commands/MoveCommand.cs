using MudServer.Server.Models;
using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public class MoveCommand(IActionManager actionManager, ILogger<MoveCommand> logger) : IGameCommand
{
    public string ActionType => "move";

    // JSON properties
    public int Priority { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }

    private readonly IActionManager actionManager = actionManager;
    private readonly ILogger<MoveCommand> logger = logger;

    public Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        var gameAction = new GameAction
        {
            Action = Actions.Move,
            TargetX = TargetX,
            TargetY = TargetY,
            Priority = Priority
        };

        this.actionManager.AddAction(gameAction);
        this.logger.LogInformation("Client {ClientId} moved to ({X},{Y})", context.ClientId, TargetX, TargetY);

        return Task.CompletedTask;
    }
}