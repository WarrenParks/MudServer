using System.Net.WebSockets;

namespace MudServer.Server.Commands;

public class WebSocketContext
{
    public WebSocket Socket { get; }
    public Guid ClientId { get; }

    public WebSocketContext(WebSocket socket, Guid clientId)
    {
        Socket = socket;
        ClientId = clientId;
    }
}