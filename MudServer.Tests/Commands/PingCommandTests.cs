using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Commands;
using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests.Commands;

public class PingCommandTests
{
    private readonly PingCommand pingCommand;

    private readonly Mock<INotificationManager> notificationManagerMock;
    private readonly Mock<Server.Commands.WebSocketContext> _contextMock;

    public PingCommandTests()
    {
        this._contextMock = new Mock<Server.Commands.WebSocketContext>(MockBehavior.Strict, new Mock<WebSocket>().Object, Guid.NewGuid());
        this.notificationManagerMock = new Mock<INotificationManager>(MockBehavior.Strict);

        var logger = new Mock<ILogger<PingCommand>>();

        this.pingCommand = new PingCommand(logger.Object, this.notificationManagerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendPongNotificationToClient()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        this.notificationManagerMock
            .Setup(n => n.NotifyClient(It.IsAny<Guid>(), It.Is<string>(s => s == "Pong! from server to user")))
            .Returns(Task.CompletedTask);

        // Act
        await this.pingCommand.ExecuteAsync(_contextMock.Object, cancellationToken);

        // Assert
        this.notificationManagerMock.Verify(n => n.NotifyClient(_contextMock.Object.ClientId, "Pong! from server to user"), Times.Once);
    }
}