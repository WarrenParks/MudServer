using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface IActionManager
{
    public void AddAction(GameAction action);

    public Task<IEnumerable<GameAction>> CollectActionsAsync(CancellationToken cancellationToken);

    public Task<GameAction> WaitForActionAsync(Actions action, CancellationToken cancellationToken);
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

    public async Task<IEnumerable<GameAction>> CollectActionsAsync(CancellationToken cancellationToken)
    {
        var collectedActions = new List<GameAction>();
        var startTime = DateTime.UtcNow;
        const int timeoutSeconds = 60;

        logger.LogInformation("Starting action collection phase");

        while ((DateTime.UtcNow - startTime).TotalSeconds < timeoutSeconds && !cancellationToken.IsCancellationRequested)
        {
            // Collect any available actions
            while (actions.TryDequeue(out var action))
            {
                collectedActions.Add(action);
            }

            try
            {
                await Task.Delay(100, cancellationToken); // Shorter delay for responsiveness
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Action collection cancelled during shutdown");
                break;
            }
        }

        logger.LogInformation("Collected {Count} actions", collectedActions.Count);
        return collectedActions;
    }

    public async Task<GameAction> WaitForActionAsync(Actions action, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Check for start game actions
            if (actions.TryDequeue(out var gameAction) && gameAction?.Action == action)
            {
                this.logger.LogInformation("Action found: {Action}", gameAction);
                return gameAction;
            }

            // wait for a short period before checking again
            try
            {
                await Task.Delay(50, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Game start wait cancelled during shutdown");
                throw;
            }
        }

        throw new OperationCanceledException(cancellationToken);
    }
}