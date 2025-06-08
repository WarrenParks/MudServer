using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MudServer;

public class Client(
  ILogger<Client> logger,
  IHttpContextAccessor httpContextAccessor,
  IConnectionManager connectionManager) : IHostedService
{
  private readonly ILogger<Client> logger = logger;
  private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
  private readonly IConnectionManager connectionManager = connectionManager;
  private readonly Guid clientId = Guid.NewGuid();

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var httpContext = this.httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null.");

    this.logger.LogInformation("Client {ClientId} started. {RemoteIp}", clientId, httpContext.Connection.RemoteIpAddress);

    using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();

    this.connectionManager.AddConnection(clientId, webSocket);
    var buffer = new byte[1024 * 4];

    while (webSocket.State == System.Net.WebSockets.WebSocketState.Open)
    {
      var messageBuffer = new List<byte>();
      System.Net.WebSockets.WebSocketReceiveResult result;
      do
      {
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        messageBuffer.AddRange(buffer.Take(result.Count));
      } while (!result.EndOfMessage);

      if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
        break;

      string message = Encoding.UTF8.GetString(messageBuffer.ToArray());
      // Parse JSON here
      try
      {
        var json = JsonDocument.Parse(message);
        this.logger.LogInformation("Client {ClientId} Received JSON: {Json}", clientId, json.RootElement);

        // Handle your JSON message here
        if (json.RootElement.TryGetProperty("action", out var action))
        {
          switch (action.GetString())
          {
            case "ping":
              // Respond to ping
              var response = new { action = "pong", this.clientId };
              var responseJson = JsonSerializer.Serialize(response);
              var responseBytes = Encoding.UTF8.GetBytes(responseJson);
              await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, cancellationToken);
              break;

            case "broadcast":
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


            // Add more actions as needed

            default:
              this.logger.LogWarning("Client {ClientId} Unknown action: {Action}", clientId, action);
              break;
          }
        }
      }
      catch (JsonException)
      {
        // Handle invalid JSON
        this.logger.LogWarning("Client {ClientId} Can't parse this: {Message}", clientId, message);
      }
    }

    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}