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

    // Ignore this test for now
    [Fact(Skip = "This test is currently skipped because notification manager not implemented.")]
    public async Task ExecuteAsync_ShouldSendPongNotificationToClient()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        await this.pingCommand.ExecuteAsync(_contextMock.Object, cancellationToken);

        // Assert
        // check notification manager was called
    }
}