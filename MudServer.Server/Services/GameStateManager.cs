using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface IGameStateManager
{
  Task WaitForStartAsync(CancellationToken cancellationToken);

  public GameState GameState { get; set; }
}

public class GameStateManager(ILogger<GameStateManager> logger) : IGameStateManager
{
  private readonly ILogger<GameStateManager> logger = logger;

  public GameState GameState { get; set; } = null!; // This should be initialized with the actual game state when the game starts

  public async Task WaitForStartAsync(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      // if (this.actions.TryDequeue(out var gameAction))
      // {
      //   if (gameAction != null && gameAction.Action == action)
      //   {
      //     this.logger.LogInformation("Action found: {Action}", gameAction);
      //     return (T)Convert.ChangeType(gameAction, typeof(T));
      //   }
      // }

      // wait for a short period before checking again
      await Task.Delay(100, cancellationToken);
    }

    // If cancelled, throw an OperationCanceledException
    throw new OperationCanceledException(cancellationToken);
  }
}