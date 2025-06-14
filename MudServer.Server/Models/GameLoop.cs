using MudServer.Server.Models;
using MudServer.Server.Services;

namespace MudServer.Models;

public class GameLoop(IActionManager actionManager)
{
  private readonly IActionManager actionManager = actionManager;

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
    await this.actionManager.WaitForActionAsync(Actions.StartGame, cancellationToken);
    // in future get this from the action manager as part of the game start action
    var gameOptions = new GameOptions
    {
      GameName = "Test Game",
      GameDescription = "This is a test game.",
      MapWidth = 10,
      MapHeight = 10,
      MaxPlayers = 4,
    };
    var gameState = new GameState(gameOptions);

    while (!cancellationToken.IsCancellationRequested)
    {
      // Turn Start Phase
      var turn = new Turn();
      CurrentPhase = Phase.TurnStart;
      OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);
      // TODO: Broadcast game state to all players


      // Action Submission Phase
      CurrentPhase = Phase.ActionSubmission;
      OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);
      turn.Actions = await this.actionManager.CollectActionsAsync(cancellationToken);

      // Action Resolution Phase
      CurrentPhase = Phase.ActionResolution;
      OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);
      // TODO: Process and resolve actions
      //this.actionManager.ProcessActions(turn.Actions);



      // Turn End Phase
      CurrentPhase = Phase.TurnEnd;
      OnPhaseChanged?.Invoke(CurrentPhase, TurnNumber);
      // TODO: Broadcast results and handle end-of-turn effects
      gameState.Turns.Add(turn);
      TurnNumber++;
    }
  }
}
