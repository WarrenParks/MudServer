namespace MudServer.Server.Models;

public class Turn
{
    public IEnumerable<GameAction> Actions { get; set; } = [];

    public IEnumerable<Outcome> Outcomes { get; set; } = [];

    public int TurnNumber { get; set; }

    public Turn(int turnNumber)
    {
        TurnNumber = turnNumber;
    }

}