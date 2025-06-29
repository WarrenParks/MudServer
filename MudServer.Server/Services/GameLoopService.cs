using MudServer.Server.Models;

namespace MudServer.Server.Services;

public class GameLoopService(
    IActionManager actionManager,
    IGameStateManager gameStateManager,
    ILogger<GameLoopService> logger,
    GameLoop gameLoop) : BackgroundService
{
    private readonly IActionManager actionManager = actionManager;
    private readonly IGameStateManager gameStateManager = gameStateManager;
    private readonly ILogger<GameLoopService> logger = logger;
    private readonly GameLoop gameLoop = gameLoop;
    private Action<GameLoop.Phase, int> phaseChangedHandler = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Game Loop Service is starting.");

        await this.actionManager.WaitForActionAsync(Actions.StartGame, stoppingToken);

        // Store the handler in a field so we can reference it for unsubscribing
        this.phaseChangedHandler = (phase, turnNumber) =>
        {
            this.logger.LogInformation("Phase changed to {Phase} for turn {TurnNumber}", phase, turnNumber);

            // Here you can add logic to broadcast the phase change to connected clients
        };

        this.gameLoop.OnPhaseChanged += this.phaseChangedHandler;

        await this.gameLoop.StartAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Game Loop Service is stopping.");

        // Safely unsubscribe from the event
        if (this.phaseChangedHandler != null)
        {
            this.gameLoop.OnPhaseChanged -= this.phaseChangedHandler;
        }

        await base.StopAsync(cancellationToken);
    }
}