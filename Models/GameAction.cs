public enum Actions
{
  Move,
  Attack,
  Defend,
  Heal,
  UseItem
}

public class GameAction
{
  public int Priority { get; set; }
  public Actions Action { get; set; }
  public string Target { get; set; }
}
