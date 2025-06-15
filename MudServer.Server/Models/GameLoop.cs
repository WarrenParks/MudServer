using MudServer.Server.Services;

namespace MudServer.Server.Models;

public class GameLoop(
  IActionManager actionManager,
  IGameStateManager gameStateManager)
{
  private readonly IActionManager actionManager = actionManager;
  private readonly IGameStateManager gameStateManager = gameStateManager;

  public enum Phase
  {
    TurnStart,
    ActionSubmission,
    ActionResolution,
    TurnEnd
  }

  public Phase CurrentPhase { get; private set; } = Phase.TurnStart;
  public int TurnNumber { get; private set; } = 1;

  public event Action<Phase, int>? OnPhaseChanged;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    // wait for the game start action to be triggered
    //var gameOptions = await this.actionManager.WaitForActionAsync<GameOptions>(Actions.StartGame, cancellationToken);
    // in future get this from the action manager as part of the game start action

    await this.gameStateManager.WaitForStartAsync(cancellationToken);

    // var gameOptions = new GameOptions
    // {
    //   GameName = "Test Game",
    //   GameDescription = "This is a test game.",
    //   MapWidth = 10,
    //   MapHeight = 10,
    //   MaxPlayers = 4,
    // };
    //var gameState = new GameState(gameOptions);

    while (!cancellationToken.IsCancellationRequested)
    {
      var turn = this.StartTurn();

      turn.Actions = await this.GetActionsForTurnAsync(cancellationToken);
      turn.Outcomes = this.ProcessActions(turn.Actions, this.gameStateManager.GameState);

      this.EndTurn(turn, this.gameStateManager.GameState);
    }
  }

  private void EndTurn(Turn turn, GameState gameState)
  {
    // Turn End Phase
    CurrentPhase = Phase.TurnEnd;
    this.OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);
    // TODO: Broadcast results and handle end-of-turn effects
    gameState.Turns.Add(turn);
    TurnNumber++;
  }

  private IEnumerable<Outcome> ProcessActions(IEnumerable<GameAction> actions, GameState gameState)
  {
    CurrentPhase = Phase.ActionResolution;
    this.OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);

    var outcomes = new List<Outcome>();
    foreach (var action in actions)
    {
      // Here you would implement the logic to process each action
      // For now, we just create a dummy outcome for each action
      var outcome = new Outcome
      {
        Action = action,
        Success = true, // Assume success for now
        Message = $"Action {action.Action} processed successfully."
      };
      outcomes.Add(outcome);
    }

    return outcomes;
  }

  private Turn StartTurn()
  {
    // Turn Start Phase
    var turn = new Turn(this.TurnNumber);
    CurrentPhase = Phase.TurnStart;
    this.OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);

    // TODO: Broadcast game state to all players
    return turn;
  }

  private async Task<IEnumerable<GameAction>> GetActionsForTurnAsync(CancellationToken cancellationToken)
  {
    // Action Submission Phase
    CurrentPhase = Phase.ActionSubmission;
    this.OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);
    return await this.actionManager.CollectActionsAsync(cancellationToken);
  }
}
