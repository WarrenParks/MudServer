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

    public Guid FromClientId { get; set; }

    public string Message { get; set; } = null!;

    public async Task ExecuteAsync(WebSocketContext context, CancellationToken cancellationToken)
    {
        if (TargetClientId == Guid.Empty)
        {
            // Send the message to all clients
            await chatManager.SendMessageAsync(this.Message, context.ClientId, cancellationToken);
        }
        else
        {
            // Send the message to the specified client
            await chatManager.SendMessageAsync(this.Message, context.ClientId, TargetClientId, cancellationToken);
        }
    }
}