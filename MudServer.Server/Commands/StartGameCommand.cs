using MudServer.Server.Models;
using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public class StartGameCommand(
  IActionManager actionManager,
  ILogger<StartGameCommand> logger) : IGameCommand
{
    public string ActionType => "startgame";

    private readonly IActionManager actionManager = actionManager;
    private readonly ILogger<StartGameCommand> logger = logger;

    public async Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        actionManager.AddAction(new GameAction
        {
            Action = Actions.StartGame,
            Priority = 1
        });

        this.logger.LogInformation("Client {ClientId} started the game.", context.ClientId);

        // Here you would typically initialize the game state, set up players, etc.
        // For now, we just simulate a game start with a simple message.

        await Task.CompletedTask; // Simulate async operation
    }
}