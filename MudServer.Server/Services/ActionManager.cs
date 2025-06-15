using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface IActionManager
{
  public void AddAction(GameAction action);

  public Task<List<GameAction>> CollectActionsAsync(CancellationToken cancellationToken);

  public Task WaitForActionAsync(Actions action, CancellationToken cancellationToken);
}

public class ActionManager(ILogger<ActionManager> logger) : IActionManager
{
  private readonly ILogger<ActionManager> logger = logger;
  private readonly Queue<GameAction> actions = [];

  public void AddAction(GameAction action)
  {
    // This method will add an action to the action manager.
    // In a real implementation, you would store this in a collection.
    // For now, we just log the action.

    logger.LogInformation("Action added: {Action}", action);
    this.actions.Enqueue(action);
  }

  /// <summary>
  /// Collects actions from players during the action submission phase.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token to stop the collection process.</param>
  /// <returns>A task that represents the asynchronous operation, containing a list of collected actions.</returns>

  public Task<List<GameAction>> CollectActionsAsync(
      //Turn turn,
      CancellationToken cancellationToken)
  {
    // This method will collect actions from the action submission phase.
    // It will wait for a specified time or until the cancellation token is triggered.

    return Task.Run(async () =>
    {
      //var actions = new List<GameAction>();
      var startTime = DateTime.UtcNow;

      while ((DateTime.UtcNow - startTime).TotalSeconds < 60 && !cancellationToken.IsCancellationRequested)
      {
        // Simulate waiting for actions to be added
        await Task.Delay(1000, cancellationToken);

        // Here we would normally check if any actions have been added
        // For now, we just return an empty list after the delay
      }

      var actions = this.actions.ToList();
      this.actions.Clear(); // Clear the actions after collecting them

      return actions;
    }, cancellationToken);
  }

  public Task WaitForActionAsync(Actions action, CancellationToken cancellationToken)
  {
    return Task.Run(async () =>
    {
      //var actions = new List<GameAction>();
      //var startTime = DateTime.UtcNow;
      var actionFound = false;

      while (!actionFound && !cancellationToken.IsCancellationRequested)
      {
        if (this.actions.TryDequeue(out var gameAction))
        {
          if (gameAction != null && gameAction.Action == action)
          {
            this.logger.LogInformation("Action found: {Action}", gameAction);
            actionFound = true;
          }
        }

        // wait for a short period before checking again
        await Task.Delay(100, cancellationToken);
      }
    }, cancellationToken);
  }
}