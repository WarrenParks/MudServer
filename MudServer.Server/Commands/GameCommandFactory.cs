using System.Text.Json;

using MudServer.Server.Services;

namespace MudServer.Server.Commands;

public interface IGameCommandFactory
{
  Task<IGameCommand?> CreateCommandAsync(string json);
}

public class GameCommandFactory(
    IServiceProvider serviceProvider,
    JsonSerializerOptions jsonOptions,
    ILogger<GameCommandFactory> logger) : IGameCommandFactory
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
  private readonly ILogger<GameCommandFactory> _logger = logger;

  // Map of action type strings to command types
  private readonly Dictionary<string, Type> _commandTypes = new()
  {
    ["move"] = typeof(MoveCommand),
    // ["attack"] = typeof(AttackCommand),
    ["ping"] = typeof(PingCommand),
    // ["chat"] = typeof(ChatCommand),
    ["startgame"] = typeof(StartGameCommand)
    // Add more mappings as you create them
  };

  public Task<IGameCommand?> CreateCommandAsync(string json)
  {
    try
    {
      // First parse to get the action type
      using var document = JsonDocument.Parse(json);
      if (!document.RootElement.TryGetProperty("action", out var actionElement))
        return Task.FromResult<IGameCommand?>(null);

      string actionType = actionElement.GetString()?.ToLower() ?? string.Empty;

      // Find matching command type
      if (!_commandTypes.TryGetValue(actionType, out var commandType))
        return Task.FromResult<IGameCommand?>(null);

      // Deserialize JSON to the command type
      var command = (IGameCommand)JsonSerializer.Deserialize(json, commandType, _jsonOptions)!;

      // If any required dependencies, inject them
      ActivatorUtilities.CreateInstance(_serviceProvider, commandType);

      return Task.FromResult<IGameCommand?>(command);
    }
    catch (JsonException ex)
    {
      _logger.LogError(ex, "Failed to create command from JSON: {Json}", json);

      return Task.FromResult<IGameCommand?>(
        new InvalidCommand($"Invalid JSON Submitted: {json}"));
    }
  }
}