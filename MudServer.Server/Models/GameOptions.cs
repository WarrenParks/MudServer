public class GameOptions
{
  public int MaxPlayers { get; set; } = 100; // Default maximum players
  public int MaxTurns { get; set; } = 1000; // Default maximum turns
  public int TurnDurationSeconds { get; set; } = 60;
  public string GameName { get; set; } = "Default Game"; // Default game name
  public string GameDescription { get; set; } = "A default game description.";
  public int MapWidth { get; set; } = 100; // Default map width
  public int MapHeight { get; set; } = 100; // Default map height
}