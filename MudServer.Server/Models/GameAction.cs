namespace MudServer.Models;

public enum Actions
{
  // Player Actions
  Move,
  Attack,
  Defend,
  Heal,
  UseItem,
  // Admin Actions
  Ping,
  StartGame,
  Broadcast,
}

public class GameAction
{
  public int Priority { get; set; }
  public Actions Action { get; set; }
  public string? Target { get; set; } // For player/entity targets (optional)
  public int? TargetX { get; set; }  // For map coordinate targets (optional)
  public int? TargetY { get; set; }  // For map coordinate targets (optional)
}
