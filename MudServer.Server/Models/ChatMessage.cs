public class ChatMessage
{
  public string Sender { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;

  public ChatMessage(string sender, string content)
  {
    Sender = sender;
    Content = content;
  }

  public override string ToString()
  {
    return $"{Timestamp:HH:mm:ss} [{Sender}]: {Content}";
  }
}