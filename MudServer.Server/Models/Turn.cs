namespace MudServer.Server.Models;

public class Turn
{
    public IEnumerable<GameAction> Actions { get; set; } = [];
    public DateTime? EndTime { get; set; }
    public IEnumerable<Outcome> Outcomes { get; set; } = [];
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public int TurnNumber { get; set; }

    public Turn(int turnNumber)
    {
        TurnNumber = turnNumber;
    }

}