using System.Text.Json;

namespace MudServer.Server.Commands;

public interface IGameCommandFactory
{
    IGameCommand CreateCommand(string json);
}

public class GameCommandFactory(
    IServiceProvider serviceProvider,
    JsonSerializerOptions jsonOptions,
    ILogger<GameCommandFactory> logger) : IGameCommandFactory
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly JsonSerializerOptions jsonOptions = jsonOptions;
    private readonly ILogger<GameCommandFactory> logger = logger;

    // Map of action type strings to command types
    private readonly Dictionary<string, Type> commandTypes = new()
    {
        ["move"] = typeof(MoveCommand),
        // ["attack"] = typeof(AttackCommand),
        ["ping"] = typeof(PingCommand),
        // ["chat"] = typeof(ChatCommand),
        ["startgame"] = typeof(StartGameCommand)
        // Add more mappings as you create them
    };

    public IGameCommand CreateCommand(string json)
    {
        try
        {
            // First parse to get the action type
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("action", out var actionElement))
            {
                // Log error and return InvalidCommand if 'action' property is missing
                logger.LogError("Missing required 'action' property in JSON: {Json}", json);
                return new InvalidCommand("Missing required 'action' property");
            }

            string actionType = actionElement.GetString()?.ToLower() ?? string.Empty;

            // Find matching command type
            if (!commandTypes.TryGetValue(actionType, out var commandType))
            {
                logger.LogError("Invalid action type: {ActionType} in JSON: {Json}", actionType, json);
                return new InvalidCommand($"Invalid action type: {actionType}");
            }

            var command = (IGameCommand)this.serviceProvider.GetService(commandType)!;

            this.PopulateObjectFromJson(document, command, commandType);

            return command;
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to create command from JSON: {Json}", json);

            return new InvalidCommand($"Invalid JSON Submitted: {json}");
        }
    }

    // Helper method to populate an existing object from JSON
    private void PopulateObjectFromJson(JsonDocument document, IGameCommand command, Type commandType)
    {
        foreach (var property in commandType.GetProperties().Where(p => p.CanWrite))
        {
            var propertyFound = false;

            // Case insensitive property lookup
            foreach (var jsonProperty in document.RootElement.EnumerateObject())
            {
                if (string.Equals(jsonProperty.Name, property.Name, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        // Convert and set the property value
                        var value = JsonSerializer.Deserialize(
                            jsonProperty.Value.GetRawText(),
                            property.PropertyType,
                            this.jsonOptions);

                        property.SetValue(command, value);
                        propertyFound = true;
                        break;
                    }
                    catch (JsonException ex)
                    {
                        this.logger.LogWarning(ex, "Failed to deserialize property {Property}", property.Name);
                    }
                }
            }

            if (!propertyFound)
            {
                this.logger.LogDebug("JSON property not found for {PropertyName}", property.Name);
            }
        }
    }
}