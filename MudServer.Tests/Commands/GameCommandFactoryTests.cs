using System;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Commands;
using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests.Commands;

public class GameCommandFactoryTests
{
  private readonly GameCommandFactory gameCommandFactory;
  private readonly Mock<IServiceProvider> serviceProviderMock;
  private readonly Mock<ILogger<GameCommandFactory>> loggerMock;

  public GameCommandFactoryTests()
  {
    this.serviceProviderMock = new Mock<IServiceProvider>();
    this.loggerMock = new Mock<ILogger<GameCommandFactory>>();
    var jsonSerializerOptions = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    this.gameCommandFactory = new GameCommandFactory(
      this.serviceProviderMock.Object,
      jsonSerializerOptions,
      this.loggerMock.Object);
  }

  [Fact]
  public async Task CreateCommandAsync_BadJson_LogsWarningReturnsInvalidCommand()
  {
    // Arrange
    var invalidJson = "this is not valid json";
    var expectedCommand = new InvalidCommand($"Invalid JSON Submitted: {invalidJson}");

    // Act
    var actualCommand = await this.gameCommandFactory.CreateCommandAsync(invalidJson);

    // Assert
    Assert.IsType<InvalidCommand>(actualCommand);
    Assert.Equal(expectedCommand.ErrorMessage, ((InvalidCommand)actualCommand).ErrorMessage);
  }

  [Fact]
  public async Task CreateCommandAsync_MoveCommand_DeserializesProperties()
  {
    // Arrange
    var moveJson = @"{""action"": ""move"", ""priority"": 5, ""targetX"": 10, ""targetY"": 20}";

    // Set up mock for MoveCommand
    var moveCommand = new MoveCommand(Mock.Of<IActionManager>(), Mock.Of<ILogger<MoveCommand>>());

    this.serviceProviderMock
        .Setup(sp => sp.GetService(typeof(MoveCommand)))
        .Returns(moveCommand);

    // Act
    var result = await this.gameCommandFactory.CreateCommandAsync(moveJson);

    // Assert
    Assert.NotNull(result);
    var actualMoveCommand = Assert.IsType<MoveCommand>(result);
    Assert.Equal(5, actualMoveCommand.Priority);
    Assert.Equal(10, actualMoveCommand.TargetX);
    Assert.Equal(20, actualMoveCommand.TargetY);
  }

  [Fact]
  public async Task CreateCommandAsync_PingCommand_CreatesWithDependencyInjection()
  {
    // Arrange
    var pingJson = @"{""action"": ""ping""}";
    var pingCommand = new PingCommand(Mock.Of<ILogger<PingCommand>>(), Mock.Of<IChatManager>());

    serviceProviderMock
        .Setup(sp => sp.GetService(typeof(PingCommand)))
        .Returns(pingCommand);

    // Act
    var result = await this.gameCommandFactory.CreateCommandAsync(pingJson);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<PingCommand>(result);
  }
}