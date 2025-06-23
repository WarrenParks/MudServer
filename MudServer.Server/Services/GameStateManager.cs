using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface IGameStateManager
{
    Task WaitForStartAsync(CancellationToken cancellationToken);

    void EndTurn(Turn turn);

    Turn StartTurn();

    public GameState GameState { get; set; }
}

public class GameStateManager(ILogger<GameStateManager> logger) : IGameStateManager
{
    private readonly ILogger<GameStateManager> logger = logger;

    public GameState GameState { get; set; } = null!; // This should be initialized with the actual game state when the game starts

    public void EndTurn(Turn turn)
    {
        // Update the game state to reflect the end of the current turn
        this.logger.LogInformation("Ending turn: {TurnId}", turn.Id);
        turn.EndTime = DateTime.UtcNow;
        this.GameState.CurrentTurn = null; // Clear the current turn
        this.logger.LogInformation("Turn ended successfully.");
    }

    public Turn StartTurn()
    {
        // Initialize and return a new turn
        var newTurn = new Turn
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.UtcNow
        };
        this.GameState.CurrentTurn = newTurn; // Set the new turn as the current turn
        this.logger.LogInformation("Starting new turn: {TurnId}", newTurn.Id);
        return newTurn;
    }

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