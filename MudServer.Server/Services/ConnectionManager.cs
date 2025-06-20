using System.Net.WebSockets;

namespace MudServer.Server.Services;

public interface IConnectionManager
{
  void AddConnection(Guid clientId, WebSocket webSocket);

  IDictionary<Guid, WebSocket> GetAllConnections();

  void RemoveConnection(Guid clientId);
}

public class ConnectionManager : IConnectionManager
{
  private readonly Dictionary<Guid, WebSocket> connections = new();

  public void AddConnection(Guid clientId, WebSocket webSocket)
  {
    if (webSocket == null) throw new ArgumentNullException(nameof(webSocket));
    if (this.connections.ContainsKey(clientId))
      throw new InvalidOperationException($"Client {clientId} is already connected.");

    this.connections[clientId] = webSocket;
  }

  public IDictionary<Guid, WebSocket> GetAllConnections()
  {
    return this.connections.ToDictionary(entry => entry.Key, entry => entry.Value);
  }

  public void RemoveConnection(Guid clientId)
  {
    if (!this.connections.Remove(clientId))
      throw new KeyNotFoundException($"Client {clientId} not found.");
  }
}