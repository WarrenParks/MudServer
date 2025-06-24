using MudServer.Server.Models;

namespace MudServer.Server.Services;

public class GameLoopService(
    IGameStateManager gameStateManager,
    ILogger<GameLoopService> logger,
    GameLoop gameLoop) : BackgroundService
{
    private readonly IGameStateManager gameStateManager = gameStateManager;
    private readonly ILogger<GameLoopService> logger = logger;
    private readonly GameLoop gameLoop = gameLoop;
    private Action<GameLoop.Phase, int> phaseChangedHandler = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Game Loop Service is starting.");

        await this.gameStateManager.WaitForStartAsync(stoppingToken);

        // Store the handler in a field so we can reference it for unsubscribing
        this.phaseChangedHandler = (phase, turnNumber) =>
        {
            this.logger.LogInformation("Phase changed to {Phase} for turn {TurnNumber}", phase, turnNumber);

            // Here you can add logic to broadcast the phase change to connected clients
        };

        this.gameLoop.OnPhaseChanged += this.phaseChangedHandler;

        await this.gameLoop.StartAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Game Loop Service is stopping.");

        // Safely unsubscribe from the event
        if (this.phaseChangedHandler != null)
        {
            this.gameLoop.OnPhaseChanged -= this.phaseChangedHandler;
        }

        return base.StopAsync(cancellationToken);
    }
}