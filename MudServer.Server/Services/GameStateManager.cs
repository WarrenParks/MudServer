using MudServer.Server.Models;

namespace MudServer.Server.Services;

public interface IGameStateManager
{
    void EndTurn(Turn turn);

    Turn StartTurn();

    public GameState GameState { get; set; }
}

public class GameStateManager(
    ILogger<GameStateManager> logger,
    IActionManager actionManager) : IGameStateManager
{
    private readonly ILogger<GameStateManager> logger = logger;
    private readonly IActionManager actionManager = actionManager;

    public GameState GameState { get; set; } = null!; // This should be initialized with the actual game state when the game starts

    public void EndTurn(Turn turn)
    {
        // Update the game state to reflect the end of the current turn
        this.logger.LogInformation("Ending turn: {TurnNumber}", turn.TurnNumber);
        turn.EndTime = DateTime.UtcNow;
        this.logger.LogInformation("Turn ended successfully.");
    }

    public Turn StartTurn()
    {
        // Initialize and return a new turn
        var newTurn = new Turn(this.GameState.Turns.Count + 1)
        {
            StartTime = DateTime.UtcNow
        };
        this.GameState.CurrentTurn = newTurn; // Set the new turn as the current turn
        this.logger.LogInformation("Starting new turn: {TurnNumber}", newTurn.TurnNumber);
        this.GameState.Turns.Add(newTurn);

        return newTurn;
    }
}