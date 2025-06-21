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

    private readonly Mock<IChatManager> chatManagerMock;
    private readonly Mock<Server.Commands.WebSocketContext> _contextMock;

    public PingCommandTests()
    {
        this._contextMock = new Mock<Server.Commands.WebSocketContext>(MockBehavior.Strict, new Mock<WebSocket>().Object, Guid.NewGuid());
        this.chatManagerMock = new Mock<IChatManager>(MockBehavior.Strict);

        var logger = new Mock<ILogger<PingCommand>>();

        this.pingCommand = new PingCommand(logger.Object, this.chatManagerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnPingResponse()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        await this.pingCommand.ExecuteAsync(_contextMock.Object, cancellationToken);

        // Assert
        this.chatManagerMock.Verify(c =>
            c.SendMessageAsync(
                It.Is<string>(s => s == "pong"),
                It.Is<Guid>(g => g == _contextMock.Object.ClientId),
                cancellationToken),
            Times.Once);
    }
}