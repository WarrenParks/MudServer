using System;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Commands;

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
  public async Task CreateCommandAsync_BadJson_LogsWarningSendsNotification()
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
}