using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MudServer.Server.Models;
using MudServer.Server.Services;

namespace MudServer;

public class Client(
  ILogger<Client> logger,
  IHttpContextAccessor httpContextAccessor,
  IConnectionManager connectionManager,
  IActionManager actionManager,
  IChatManager chatManager,
  JsonSerializerOptions jsonSerializerOptions) : IHostedService
{
  private readonly ILogger<Client> logger = logger;
  private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
  private readonly IConnectionManager connectionManager = connectionManager;
  private readonly IActionManager actionManager = actionManager;
  private readonly IChatManager chatManager = chatManager;
  private readonly JsonSerializerOptions jsonSerializerOptions = jsonSerializerOptions;
  private readonly Guid clientId = Guid.NewGuid();

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var httpContext = this.httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null.");
    this.logger.LogInformation("Client {ClientId} started. {RemoteIp}", clientId, httpContext.Connection.RemoteIpAddress);

    using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
    this.connectionManager.AddConnection(clientId, webSocket);

    try
    {
      await HandleWebSocketCommunication(webSocket, cancellationToken);
    }
    finally
    {
      connectionManager.RemoveConnection(clientId);
      if (webSocket.State != WebSocketState.Closed)
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
    }

    this.logger.LogInformation("Client {ClientId} disconnected.", clientId);
  }

  private async Task HandleWebSocketCommunication(WebSocket webSocket, CancellationToken cancellationToken)
  {
    var buffer = new byte[1024 * 4];

    while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
    {
      var message = await ReceiveFullMessage(webSocket, buffer, cancellationToken);
      if (message == null) break; // Connection closed

      await ProcessMessage(message, webSocket, cancellationToken);
    }
  }

  private async Task<string?> ReceiveFullMessage(WebSocket webSocket, byte[] buffer, CancellationToken cancellationToken)
  {
    var messageBuffer = new List<byte>();
    WebSocketReceiveResult result;

    do
    {
      result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
      messageBuffer.AddRange(buffer.Take(result.Count));
    } while (!result.EndOfMessage);

    if (result.MessageType == WebSocketMessageType.Close)
      return null;

    return Encoding.UTF8.GetString(messageBuffer.ToArray());
  }

  private async Task ProcessMessage(string message, WebSocket webSocket, CancellationToken cancellationToken)
  {
    try
    {
      var jsonDocument = JsonDocument.Parse(message);
      logger.LogInformation("Client {ClientId} Received JSON: {Json}", clientId, jsonDocument.RootElement);

      var gameAction = jsonDocument.Deserialize<GameAction>(jsonSerializerOptions);

      if (gameAction != null)
        await ProcessGameAction(gameAction, jsonDocument, webSocket, cancellationToken);
    }
    catch (JsonException je)
    {
      logger.LogWarning("Client {ClientId} Can't parse this: {Message}, exception message: {ExMes}",
          clientId, message, je.Message);
    }
  }

  private async Task ProcessGameAction(
    GameAction gameAction,
    JsonDocument jsonDocument,
    WebSocket webSocket,
    CancellationToken cancellationToken)
  {
    switch (gameAction.Action)
    {
      case Actions.Ping:
        await HandlePingAction(webSocket, cancellationToken);
        break;

      case Actions.Broadcast:
        HandleBroadcastAction(jsonDocument, cancellationToken);
        break;

      case Actions.Defend:
      case Actions.Heal:
      case Actions.UseItem:
      case Actions.Move:
      case Actions.Attack:
        HandleGameplayAction(gameAction);
        break;

      case Actions.StartGame:
        HandleStartGameAction();
        break;

      default:
        logger.LogWarning("Client {ClientId} Unknown action: {Action}", clientId, gameAction.Action);
        break;
    }
  }

  private void HandleBroadcastAction(JsonDocument jsonDocument, CancellationToken cancellationToken)
  {
    var chatMessage = jsonDocument.Deserialize<ChatMessage>(jsonSerializerOptions);
    if (chatMessage != null)
    {
      //this.chatManager.BroadcastMessage(chatMessage);
    }
  }

  private void HandleGameplayAction(GameAction gameAction)
  {
    actionManager.AddAction(gameAction);
    logger.LogInformation("Client {ClientId} added action {GameAction} on ({X}, {Y})",
        clientId, gameAction.Action, gameAction.TargetX, gameAction.TargetY);
  }

  private void HandleStartGameAction()
  {
    actionManager.AddAction(new GameAction
    {
      Action = Actions.StartGame,
      Priority = 1
    });
    logger.LogInformation("Client {ClientId} started the game.", clientId);
  }

  private async Task HandlePingAction(WebSocket webSocket, CancellationToken cancellationToken)
  {
    var response = new { action = "pong", clientId };
    var responseJson = JsonSerializer.Serialize(response);
    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}