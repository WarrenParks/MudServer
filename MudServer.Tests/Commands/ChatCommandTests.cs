using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Commands;
using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests.Commands;

public class ChatCommandTests
{
    private readonly Mock<ILogger<ChatCommand>> loggerMock;
    private readonly Mock<IChatManager> chatManagerMock;
    private readonly WebSocketContext webContext;
    private readonly CancellationToken cancellationToken;

    public ChatCommandTests()
    {
        this.loggerMock = new Mock<ILogger<ChatCommand>>();
        this.chatManagerMock = new Mock<IChatManager>();
        this.webContext = new WebSocketContext(null, Guid.NewGuid());
        this.cancellationToken = new CancellationToken();

        this.chatManagerMock
            .Setup(cm => cm.SendMessageAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendMessageToChatManager()
    {
        // Arrange
        var chatCommand = new ChatCommand(chatManagerMock.Object)
        {
            Message = "Hello World",
            FromClientId = Guid.NewGuid()
        };

        // Act
        await chatCommand.ExecuteAsync(this.webContext, cancellationToken);

        // Assert
        chatManagerMock.Verify(cm => cm.SendMessageAsync("Hello World", this.webContext.ClientId, this.cancellationToken), Times.Once);
    }
}