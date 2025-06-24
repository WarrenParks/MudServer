namespace MudServer.Server.Commands;

public class InvalidCommand(string errorMessage) : IGameCommand
{
  public string ErrorMessage { get; } = errorMessage;

  public string ActionType => "InvalidCommand";

  public Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
  {
    // Log the invalid command attempt
    // Console.WriteLine($"Invalid command received from client {context.ClientId}");

    // Optionally, you can send a response back to the client
    return Task.CompletedTask;
  }
}