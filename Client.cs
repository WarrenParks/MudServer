using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MudServer;

public class Client(
  ILogger<Client> logger,
  IHttpContextAccessor httpContextAccessor) : IHostedService
{
  private readonly ILogger<Client> logger = logger;
  private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    var httpContext = this.httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null.");

    this.logger.LogInformation("Client started. {0}", httpContext.Connection.RemoteIpAddress);
    using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
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
        Debug.WriteLine($"Received JSON: {json.RootElement}");
        // Handle your JSON message here
      }
      catch (JsonException)
      {
        // Handle invalid JSON
      }
    }

    await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}