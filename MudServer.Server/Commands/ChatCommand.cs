using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public class ChatCommand(IChatManager chatManager) : IGameCommand
{
    private readonly IChatManager chatManager = chatManager;


    public string ActionType => "chat";

    /// <summary>
    /// The ID of the client to whom the message is directed. If you want to send a message to all clients, this can be 
    /// set to Guid.Empty or null.
    /// </summary>
    public Guid TargetClientId { get; set; }

    public required Guid FromClientId { get; set; }

    public required string Message { get; set; }

    public async Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        // Here you would typically send the chat message to the chat manager or similar service
        // For example:
        if (TargetClientId == Guid.Empty || TargetClientId == null)
        {
            // Broadcast the message to all clients
            await chatManager.BroadcastMessageAsync(this.Message, cancellationToken);
        }
        else
        {
            // Send the message to the specified client
            await chatManager.SendMessageAsync(this.Message, TargetClientId, cancellationToken);
        }
    }
}