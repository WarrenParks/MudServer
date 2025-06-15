namespace MudServer.Server.Models;

public class Outcome
{
  public GameAction Action { get; internal set; }

  public string Message { get; internal set; }

  public bool Success { get; internal set; }
}