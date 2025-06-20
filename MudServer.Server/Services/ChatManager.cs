public interface IChatManager
{
  public void SendMessage(string message);
}

public class ChatManager(
  ILogger<ChatManager> logger) : IChatManager
{
  private readonly ILogger<ChatManager> logger = logger;

  public void SendMessage(string message)
  {

  }
}