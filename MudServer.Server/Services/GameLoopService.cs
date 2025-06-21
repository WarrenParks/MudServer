using MudServer.Server.Models;

public class GameLoopService(
  ILogger<GameLoopService> logger,
  GameLoop gameLoop) : BackgroundService
{
    private readonly ILogger<GameLoopService> logger = logger;
    private readonly GameLoop gameLoop = gameLoop;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Game Loop Service is starting.");

        this.gameLoop.OnPhaseChanged += (phase, turnNumber) =>
        {
            this.logger.LogInformation("Phase changed to {Phase} for turn {TurnNumber}", phase, turnNumber);

            // Here you can add logic to broadcast the phase change to connected clients
        };

        return this.gameLoop.StartAsync(stoppingToken);
    }
}