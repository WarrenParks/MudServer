namespace MudServer.Server.Models;

public class Outcome
{
    public GameAction Action { get; internal set; } = null!;

    public string Message { get; internal set; } = null!;

    public bool Success { get; internal set; }
}