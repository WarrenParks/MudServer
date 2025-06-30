using MudServer.Server.Models;
using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public class RegisterCommand(
    IConnectionManager connectionManager,
    INotificationManager notificationManager,
    IWebSocketMessenger webSocketMessenger,
    ILogger<RegisterCommand> logger) : IGameCommand
{
    public string ActionType => "register";

    // JSON properties
    public string Username { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    private readonly IConnectionManager connectionManager = connectionManager;
    private readonly INotificationManager notificationManager = notificationManager;
    private readonly IWebSocketMessenger webSocketMessenger = webSocketMessenger;
    private readonly ILogger<RegisterCommand> logger = logger;

    public async Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Client {ClientId} attempting to register with username {Username}",
            context.ClientId, Username);

        if (string.IsNullOrWhiteSpace(Username))
        {
            await this.notificationManager.NotifyClientAsync(context.ClientId,
                "Username cannot be empty", cancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            await this.notificationManager.NotifyClientAsync(context.ClientId,
                "Key cannot be empty", cancellationToken);
            return;
        }

        var success = this.connectionManager.RegisterUser(context.ClientId, Username, Key);

        if (success)
        {
            // Send success response to the user
            await this.notificationManager.NotifyClientAsync(context.ClientId,
                $"Successfully registered as {Username}", cancellationToken);

            // Notify all other users that someone joined
            await this.webSocketMessenger.SendMessageAsync(
                Actions.UserJoined,
                new { username = Username, clientId = context.ClientId },
                context.ClientId, // Exclude the user who just registered
                cancellationToken);

            this.logger.LogInformation("Client {ClientId} successfully registered as {Username}",
                context.ClientId, Username);
        }
        else
        {
            string errorMessage = this.connectionManager.IsUsernameTaken(Username)
                ? "Username is already taken"
                : "Registration failed";

            await this.webSocketMessenger.SendMessageAsync(
              Actions.Error, errorMessage, context.ClientId, cancellationToken);

            this.logger.LogWarning("Registration failed for client {ClientId} with username {Username}",
                context.ClientId, Username);
        }
    }
}