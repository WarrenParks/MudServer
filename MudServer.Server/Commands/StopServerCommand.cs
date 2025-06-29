namespace MudServer.Server.Commands;

public class StopServerCommand(
    IHostApplicationLifetime applicationLifetime,
    ILogger<StopServerCommand> logger) : IGameCommand
{
  public string ActionType => "stopserver";

  private readonly IHostApplicationLifetime applicationLifetime = applicationLifetime;
  private readonly ILogger<StopServerCommand> logger = logger;

  public Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
  {
    logger.LogWarning("Server stop requested by client {ClientId}", context.ClientId);

    // Gracefully stop the application
    applicationLifetime.StopApplication();

    return Task.CompletedTask;
  }
}