public class ChatMessage
{
  public string To { get; set; } = string.Empty;
  //public Guid Sender { get; set; } = Guid.Empty;

  public string Content { get; set; } = string.Empty;

  public DateTime Timestamp { get; } = DateTime.UtcNow;

  public ChatMessage(string content)
  {
    //  Sender = sender;
    Content = content;
  }

  // public override string ToString()
  // {
  //   return $"{Timestamp:HH:mm:ss} [{Sender}]: {Content}";
  // }
}