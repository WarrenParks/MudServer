namespace MudServer.Server.Models;

public enum Actions
{
    // Player Actions
    Move,
    Attack,
    Defend,
    Heal,
    UseItem,
    // Admin Actions
    Ping,
    StartGame,
    Broadcast,
    // Server Responses
    Welcome,
    Chat,
    UserJoined,
    UserLeft,
    Notification,
    Error
}