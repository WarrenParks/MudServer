using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using MudServer.Server.Commands;
using MudServer.Server.Services;

namespace MudServer;

public class WebSocketConnectionHandler(
  IGameCommandFactory commandFactory,
  ILogger<WebSocketConnectionHandler> logger,
  IHttpContextAccessor httpContextAccessor,
  IConnectionManager connectionManager) : IHostedService
{
    private readonly IGameCommandFactory commandFactory = commandFactory;
    private readonly ILogger<WebSocketConnectionHandler> logger = logger;
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
    private readonly IConnectionManager connectionManager = connectionManager;
    private readonly Guid clientId = Guid.NewGuid();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var httpContext = this.httpContextAccessor.HttpContext ?? throw new InvalidOperationException("HttpContext is null.");
        this.logger.LogInformation("WebSocket Connection started. ClientId: {ClientId}, RemoteIp: {RemoteIp}", clientId, httpContext.Connection.RemoteIpAddress);

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("ClientId: {clientId} - Stop Async was called. Hows that work?", clientId);
        throw new NotImplementedException();
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
            var command = this.commandFactory.CreateCommand(message);

            if (command != null)
            {
                await command.ExecuteAsync(new(webSocket, clientId), cancellationToken);
            }
            else
            {
                logger.LogWarning("Client {ClientId} sent unrecognized message: {Message}", clientId, message);
            }
        }
        catch (JsonException je)
        {
            logger.LogWarning("Client {ClientId} Can't parse this: {Message}, exception message: {ExMes}",
                clientId, message, je.Message);
        }
    }


}