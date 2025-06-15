using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MudServer.Server.Models;
using MudServer.Server.Services;
using MudServer.Services;

namespace MudServer;

public class Client(
  ILogger<Client> logger,
  IHttpContextAccessor httpContextAccessor,
  IConnectionManager connectionManager,
  IActionManager actionManager,
  JsonSerializerOptions jsonSerializerOptions) : IHostedService
{
  private readonly ILogger<Client> logger = logger;
  private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
  private readonly IConnectionManager connectionManager = connectionManager;
  private readonly IActionManager actionManager = actionManager;
  private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions;
  private readonly Guid clientId = Guid.NewGuid();

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var httpContext = this.httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null.");

    this.logger.LogInformation("Client {ClientId} started. {RemoteIp}", clientId, httpContext.Connection.RemoteIpAddress);

    using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

    this.connectionManager.AddConnection(clientId, webSocket);
    var buffer = new byte[1024 * 4];

    while (webSocket.State == WebSocketState.Open)
    {
      var messageBuffer = new List<byte>();
      WebSocketReceiveResult result;

      do
      {
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        messageBuffer.AddRange(buffer.Take(result.Count));
      } while (!result.EndOfMessage);

      if (result.MessageType == WebSocketMessageType.Close)
        break;

      string message = Encoding.UTF8.GetString(messageBuffer.ToArray());
      // Parse JSON here
      try
      {
        var json = JsonDocument.Parse(message);
        this.logger.LogInformation("Client {ClientId} Received JSON: {Json}", clientId, json.RootElement);

        var gameAction = json.Deserialize<GameAction>(this.jsonSerializerOptions);

        // Handle your JSON message here
        if (gameAction != null)
        {
          switch (gameAction.Action)
          {
            case Actions.Ping:
              // Respond to ping
              var response = new { action = "pong", this.clientId };
              var responseJson = JsonSerializer.Serialize(response);
              var responseBytes = Encoding.UTF8.GetBytes(responseJson);
              await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, cancellationToken);
              break;

            case Actions.Broadcast:
              // Handle broadcast action
              if (json.RootElement.TryGetProperty("message", out var messageContent))
              {
                var broadcastMessage = new { action = "broadcast", this.clientId, message = messageContent.GetString() };
                var broadcastJson = JsonSerializer.Serialize(broadcastMessage);
                var broadcastBytes = Encoding.UTF8.GetBytes(broadcastJson);

                // Broadcast to all connected clients
                foreach (var connection in this.connectionManager.GetAllConnections())
                {
                  if (connection.Key != this.clientId) // Don't send to self
                  {
                    await connection.Value.SendAsync(new ArraySegment<byte>(broadcastBytes), WebSocketMessageType.Text, true, cancellationToken);
                  }
                }
              }
              break;

            case Actions.Defend:
            case Actions.Heal:
            case Actions.UseItem:
            case Actions.Move:
            case Actions.Attack:
              this.actionManager.AddAction(gameAction);
              this.logger.LogInformation("Client {ClientId} added action {gameAction} on ({X}, {Y})", clientId, gameAction.Action, gameAction.TargetX, gameAction.TargetY);
              break;

            case Actions.StartGame:
              this.actionManager.AddAction(new GameAction()
              {
                Action = Actions.StartGame,
                Priority = 1
              });
              this.logger.LogInformation("Client {ClientId} started the game.", clientId);
              break;

            default:
              this.logger.LogWarning("Client {ClientId} Unknown", clientId);
              break;
          }
        }
      }
      catch (JsonException je)
      {
        // Handle invalid JSON
        this.logger.LogWarning("Client {ClientId} Can't parse this: {Message}, exception message: {ExMes}", clientId, message, je.Message);
      }
    }

    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}