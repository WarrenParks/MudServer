using System;
using System.Text.Json;

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
  public void CreateCommand_NoActionProperty_ReturnsInvalidCommand()
  {
    // Arrange
    var jsonWithoutAction = @"{""someProperty"": ""value""}";
    var expectedError = "Missing required 'action' property";

    // Act
    var result = this.gameCommandFactory.CreateCommand(jsonWithoutAction);

    // Assert
    Assert.IsType<InvalidCommand>(result);
    Assert.Equal(expectedError, ((InvalidCommand)result).ErrorMessage);
    this.loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
  }

  [Fact]
  public void CreateCommand_ActionTypeNotMapped_ReturnsInvalidCommand()
  {
    // Arrange
    var unmappedActionJson = @"{""action"": ""unknownAction""}";
    var expectedError = $"Invalid action type: unknownaction"; // actions are lowercased in the factory

    // Act
    var result = this.gameCommandFactory.CreateCommand(unmappedActionJson);

    // Assert
    Assert.IsType<InvalidCommand>(result);
    Assert.Equal(expectedError, ((InvalidCommand)result).ErrorMessage);
    this.loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
  }

  [Fact]
  public void CreateCommand_BadJson_LogsWarningReturnsInvalidCommand()
  {
    // Arrange
    var invalidJson = "this is not valid json";
    var expectedCommand = new InvalidCommand($"Invalid JSON Submitted: {invalidJson}");

    // Act
    var actualCommand = this.gameCommandFactory.CreateCommand(invalidJson);

    // Assert
    Assert.IsType<InvalidCommand>(actualCommand);
    Assert.Equal(expectedCommand.ErrorMessage, ((InvalidCommand)actualCommand).ErrorMessage);
  }

  [Fact]
  public void CreateCommand_MoveCommand_DeserializesProperties()
  {
    // Arrange
    var moveJson = @"{""action"": ""move"", ""priority"": 5, ""targetX"": 10, ""targetY"": 20}";

    // Set up mock for MoveCommand
    var moveCommand = new MoveCommand(Mock.Of<IActionManager>(), Mock.Of<ILogger<MoveCommand>>());

    this.serviceProviderMock
        .Setup(sp => sp.GetService(typeof(MoveCommand)))
        .Returns(moveCommand);

    // Act
    var result = this.gameCommandFactory.CreateCommand(moveJson);

    // Assert
    Assert.NotNull(result);
    var actualMoveCommand = Assert.IsType<MoveCommand>(result);
    Assert.Equal(5, actualMoveCommand.Priority);
    Assert.Equal(10, actualMoveCommand.TargetX);
    Assert.Equal(20, actualMoveCommand.TargetY);
  }

  [Fact]
  public void CreateCommand_PingCommand_CreatesWithDependencyInjection()
  {
    // Arrange
    var pingJson = @"{""action"": ""ping""}";
    var pingCommand = new PingCommand(Mock.Of<ILogger<PingCommand>>(), Mock.Of<INotificationManager>());

    serviceProviderMock
        .Setup(sp => sp.GetService(typeof(PingCommand)))
        .Returns(pingCommand);

    // Act
    var result = this.gameCommandFactory.CreateCommand(pingJson);

    // Assert
    Assert.NotNull(result);
    Assert.IsType<PingCommand>(result);
  }
}