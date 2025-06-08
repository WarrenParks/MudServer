using System.Net.WebSockets;

public interface IConnectionManager
{
  void AddConnection(Guid clientId, WebSocket webSocket);

  IDictionary<Guid, WebSocket> GetAllConnections();

  void RemoveConnection(Guid clientId);
}