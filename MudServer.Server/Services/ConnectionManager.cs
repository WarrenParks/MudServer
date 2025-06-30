using System.Net.WebSockets;

namespace MudServer.Server.Services;

public interface IConnectionManager
{
    void AddConnection(Guid clientId, WebSocket webSocket);
    bool RegisterUser(Guid clientId, string username, string key);
    bool IsUserRegistered(Guid clientId);
    User? GetUser(Guid clientId);
    User? GetUserByName(string username);
    IDictionary<Guid, WebSocket> GetAllConnections();
    IEnumerable<User> GetAllUsers();
    IEnumerable<User> GetRegisteredUsers();
    WebSocket? GetConnection(Guid clientId);
    string GetIdentity(Guid clientId);
    void RemoveConnection(Guid clientId);
    bool IsUsernameTaken(string username);
}

public class ConnectionManager : IConnectionManager
{
    private readonly Dictionary<Guid, WebSocket> connections = new();
    private readonly Dictionary<Guid, User> users = new();
    private readonly Dictionary<string, Guid> usernameToClientId = new();
    private readonly ILogger<ConnectionManager> logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        this.logger = logger;
    }

    public void AddConnection(Guid clientId, WebSocket webSocket)
    {
        if (webSocket == null) throw new ArgumentNullException(nameof(webSocket));
        if (this.connections.ContainsKey(clientId))
            throw new InvalidOperationException($"Client {clientId} is already connected.");

        this.connections[clientId] = webSocket;

        // Create an unregistered user entry
        this.users[clientId] = new User
        {
            Id = clientId,
            Name = string.Empty,
            Key = string.Empty,
            IsRegistered = false,
            ConnectedAt = DateTime.UtcNow
        };

        this.logger.LogInformation("Client {ClientId} connected but not yet registered", clientId);
    }

    public bool RegisterUser(Guid clientId, string username, string key)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            this.logger.LogWarning("Registration failed for {ClientId}: Username cannot be empty", clientId);
            return false;
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            this.logger.LogWarning("Registration failed for {ClientId}: Key cannot be empty", clientId);
            return false;
        }

        if (!this.connections.ContainsKey(clientId))
        {
            this.logger.LogWarning("Registration failed: Client {ClientId} not found", clientId);
            return false;
        }

        if (this.IsUsernameTaken(username))
        {
            this.logger.LogWarning("Registration failed for {ClientId}: Username {Username} is already taken", clientId, username);
            return false;
        }

        // Check if user is already registered
        if (this.users.TryGetValue(clientId, out var existingUser) && existingUser.IsRegistered)
        {
            this.logger.LogWarning("Registration failed: Client {ClientId} is already registered as {Username}", clientId, existingUser.Name);
            return false;
        }

        // Update user information
        this.users[clientId] = new User
        {
            Id = clientId,
            Name = username,
            Key = key, // In production, you should hash this
            IsRegistered = true,
            ConnectedAt = existingUser?.ConnectedAt ?? DateTime.UtcNow
        };

        this.usernameToClientId[username.ToLower()] = clientId;

        this.logger.LogInformation("Client {ClientId} successfully registered as {Username}", clientId, username);
        return true;
    }

    public bool IsUserRegistered(Guid clientId)
    {
        return this.users.TryGetValue(clientId, out var user) && user.IsRegistered;
    }

    public User? GetUser(Guid clientId)
    {
        return this.users.TryGetValue(clientId, out var user) ? user : null;
    }

    public User? GetUserByName(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        if (this.usernameToClientId.TryGetValue(username.ToLower(), out var clientId))
        {
            if (this.users.TryGetValue(clientId, out var user))
            {
                return user;
            }

            this.logger.LogWarning("Username {Username} with clientId {ClientId} not found in users dictionary", username, clientId);
            return null;
        }

        this.logger.LogWarning("Username {Username} not found in usernameToClientId dictionary", username);
        return null;
    }

    public IDictionary<Guid, WebSocket> GetAllConnections()
    {
        return this.connections.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    public IEnumerable<User> GetAllUsers()
    {
        return this.users.Values.ToList();
    }

    public IEnumerable<User> GetRegisteredUsers()
    {
        return this.users.Values.Where(u => u.IsRegistered).ToList();
    }

    public WebSocket? GetConnection(Guid clientId)
    {
        return this.connections.TryGetValue(clientId, out var webSocket) ? webSocket : null;
    }

    public string GetIdentity(Guid clientId)
    {
        if (this.users.TryGetValue(clientId, out var user))
        {
            return user.IsRegistered ? user.Name : $"Anonymous_{clientId.ToString()[..8]}";
        }

        return $"Unknown_{clientId.ToString()[..8]}";
    }

    public void RemoveConnection(Guid clientId)
    {
        var userRemoved = false;
        var connectionRemoved = this.connections.Remove(clientId);

        if (this.users.TryGetValue(clientId, out var user))
        {
            if (user.IsRegistered && !string.IsNullOrEmpty(user.Name))
            {
                this.usernameToClientId.Remove(user.Name.ToLower());
            }
            userRemoved = this.users.Remove(clientId);
        }

        if (!connectionRemoved && !userRemoved)
        {
            this.logger.LogWarning("Attempted to remove non-existent client {ClientId}", clientId);
        }
        else
        {
            this.logger.LogInformation("Client {ClientId} ({Identity}) disconnected", clientId, user?.Name ?? "Anonymous");
        }
    }

    public bool IsUsernameTaken(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return true;
        return this.usernameToClientId.ContainsKey(username.ToLower());
    }
}