using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using MudServer.Server.Models;
using MudServer.Server.Services;

public interface IWebSocketMessenger
{
    Task SendMessageAsync(Actions action, string message, Guid fromClientId, CancellationToken cancellationToken);
    Task SendMessageAsync(Actions action, string message, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken);
    Task SendMessageAsync<T>(Actions action, T payload, Guid fromClientId, CancellationToken cancellationToken) where T : class;
    Task SendMessageAsync<T>(Actions action, T payload, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken) where T : class;

}

public class WebSocketMessenger(
    IConnectionManager connectionManager,
    ILogger<WebSocketMessenger> logger) : IWebSocketMessenger
{
    private readonly IConnectionManager connectionManager = connectionManager;
    private readonly ILogger<WebSocketMessenger> logger = logger;

    public async Task SendMessageAsync(
        Actions action, string message, Guid fromClientId, CancellationToken cancellationToken)
    {
        await SendMessageAsync(action, new { message }, fromClientId, cancellationToken);
    }

    public async Task SendMessageAsync(
        Actions action, string message, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken)
    {
        await this.SendMessageAsync(action, new { message }, fromClientId, toClientId, cancellationToken);
    }

    public async Task SendMessageAsync<T>(Actions action, T payload, Guid fromClientId, CancellationToken cancellationToken) where T : class
    {
        var connections = connectionManager.GetAllConnections();
        var response = CreateMergedResponse(action, payload, fromClientId);

        var tasks = connections
            .Where(kvp =>
                kvp.Key != fromClientId &&
                kvp.Value != null &&
                kvp.Value.State == WebSocketState.Open)
            .Select(kvp => SendJsonAsync(kvp.Value, response, cancellationToken));

        await Task.WhenAll(tasks);
    }

    public async Task SendMessageAsync<T>(Actions action, T payload, Guid fromClientId, Guid toClientId, CancellationToken cancellationToken) where T : class
    {
        var webSocket = connectionManager.GetConnection(toClientId);
        if (webSocket == null || webSocket.State != WebSocketState.Open)
        {
            logger.LogWarning("Cannot send message to {ClientId}: connection not found or not open. state: {State}", toClientId, webSocket?.State);
            return;
        }

        var response = CreateMergedResponse(action, payload, fromClientId, toClientId);
        await SendJsonAsync(webSocket, response, cancellationToken);
    }

    private object CreateMergedResponse(Actions action, object data, Guid fromClientId, Guid? toClientId = null)
    {
        // Start with base response
        var baseResponse = new Dictionary<string, object>
        {
            ["action"] = action.ToString().ToLower(),
            ["timestamp"] = DateTime.UtcNow.ToString("o"), // ISO 8601 format
            ["fromClientId"] = fromClientId.ToString(),
        };

        // If toClientId is provided, add it to the response
        if (toClientId.HasValue)
        {
            baseResponse["toClientId"] = toClientId.Value.ToString();
        }

        // If data is null, return just the base response
        if (data == null)
        {
            return baseResponse;
        }

        // Serialize the data object to JSON, then deserialize as Dictionary to merge properties
        var dataJson = JsonSerializer.Serialize(data);
        var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(dataJson);

        if (dataDict != null)
        {
            // Merge data properties into base response
            foreach (var kvp in dataDict)
            {
                baseResponse[kvp.Key] = kvp.Value;
            }
        }

        return baseResponse;
    }

    private async Task SendJsonAsync(WebSocket webSocket, object response, CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(response);
            var bytes = Encoding.UTF8.GetBytes(json);

            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                cancellationToken);

            logger.LogDebug("Sent message: {Json}", json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send WebSocket message");
        }
    }
}