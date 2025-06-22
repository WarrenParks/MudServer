using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Commands;
using MudServer.Server.Models;
using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests.Commands;

public class MoveCommandTests
{
  private readonly MoveCommand moveCommand;

  private readonly Mock<IActionManager> actionManagerMock;
  private readonly Mock<ILogger<MoveCommand>> loggerMock;

  private readonly WebSocketContext webContext;
  private readonly CancellationToken cancellationToken;

  public MoveCommandTests()
  {
    this.actionManagerMock = new Mock<IActionManager>();
    this.loggerMock = new Mock<ILogger<MoveCommand>>();
    this.moveCommand = new MoveCommand(actionManagerMock.Object, loggerMock.Object);
    webContext = new WebSocketContext(null, Guid.NewGuid());

    this.cancellationToken = new CancellationToken();
  }

  [Fact]
  public async Task ExecuteAsync_ShouldReturnSuccess_WhenMoveIsValid()
  {
    // Arrange
    this.moveCommand.Priority = 1;
    this.moveCommand.TargetX = 5;
    this.moveCommand.TargetY = 10;

    // Act
    await this.moveCommand.ExecuteAsync(this.webContext, this.cancellationToken);

    // Assert
    this.actionManagerMock.Verify(am => am.AddAction(It.Is<GameAction>(ga =>
        ga.Action == Actions.Move &&
        ga.TargetX == this.moveCommand.TargetX &&
        ga.TargetY == this.moveCommand.TargetY &&
        ga.Priority == this.moveCommand.Priority)), Times.Once);
  }
}