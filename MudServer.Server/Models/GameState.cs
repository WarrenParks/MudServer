namespace MudServer.Server.Models;

public class GameState(GameOptions gameOptions)
{
    GameMap GameMap { get; set; } = new GameMap(gameOptions.MapWidth, gameOptions.MapHeight);
    //IList<PlayerState> Players { get; set; } = new List<PlayerState>();

    public Turn CurrentTurn { get; set; } = null!;

    public IList<Turn> Turns { get; set; } = new List<Turn>();
}