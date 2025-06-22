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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Game Loop Service is starting.");

        await this.gameStateManager.WaitForStartAsync(stoppingToken);

        this.gameLoop.OnPhaseChanged += (phase, turnNumber) =>
        {
            this.logger.LogInformation("Phase changed to {Phase} for turn {TurnNumber}", phase, turnNumber);

            // Here you can add logic to broadcast the phase change to connected clients
        };

        await this.gameLoop.StartAsync(stoppingToken);
    }
}