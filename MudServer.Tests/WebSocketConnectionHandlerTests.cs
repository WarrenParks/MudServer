using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Commands;
using MudServer.Server.Models;
using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests;

public class WebSocketConnectionHandlerTests
{
    private readonly Mock<ILogger<WebSocketConnectionHandler>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IConnectionManager> _connectionManagerMock;
    private readonly Mock<IActionManager> _actionManagerMock;
    private readonly Mock<IGameCommandFactory> _gameCommandFactoryMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<WebSocket> _webSocketMock;
    private readonly Mock<ConnectionInfo> _connectionInfoMock;

    public WebSocketConnectionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<WebSocketConnectionHandler>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _connectionManagerMock = new Mock<IConnectionManager>();
        _actionManagerMock = new Mock<IActionManager>();
        _gameCommandFactoryMock = new Mock<IGameCommandFactory>();

        _httpContextMock = new Mock<HttpContext>();
        _webSocketMock = new Mock<WebSocket>();
        _connectionInfoMock = new Mock<ConnectionInfo>();

        // Setup default behaviors
        _httpContextAccessorMock.Setup(h => h.HttpContext)
            .Returns(_httpContextMock.Object);

        _httpContextMock.Setup(c => c.WebSockets)
            .Returns(new TestWebSocketManager(_webSocketMock.Object));

        _httpContextMock.Setup(c => c.Connection)
            .Returns(_connectionInfoMock.Object);

        _connectionInfoMock.Setup(c => c.RemoteIpAddress)
            .Returns(IPAddress.Parse("127.0.0.1"));
    }

    [Fact]
    public async Task StartAsync_AddsAndRemovesConnection()
    {
        // Arrange
        SetupWebSocketToReturnCloseMessage();
        var client = CreateClient();

        // Act
        await client.StartAsync(CancellationToken.None);

        // Assert
        _connectionManagerMock.Verify(m => m.AddConnection(It.IsAny<Guid>(), _webSocketMock.Object), Times.Once);
        _connectionManagerMock.Verify(m => m.RemoveConnection(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessage_PingAction_SendsPongResponse()
    {
        // Arrange
        byte[] sentBytes = null;
        WebSocketMessageType sentMessageType = WebSocketMessageType.Close;
        bool sentEndOfMessage = false;

        // Configure WebSocket to return a Ping message and capture the response
        SetupWebSocketWithMessage(CreateJsonMessage(Actions.Ping));
        _webSocketMock.Setup(w => w.SendAsync(It.IsAny<ArraySegment<byte>>(),
                                             It.IsAny<WebSocketMessageType>(),
                                             It.IsAny<bool>(),
                                             It.IsAny<CancellationToken>()))
                      .Callback<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken>(
                          (bytes, messageType, endOfMessage, token) =>
                          {
                              sentBytes = bytes.ToArray();
                              sentMessageType = messageType;
                              sentEndOfMessage = endOfMessage;
                          })
                      .Returns(Task.CompletedTask);

        var client = CreateClient();

        // Act
        await client.StartAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(sentBytes);
        Assert.Equal(WebSocketMessageType.Text, sentMessageType);
        Assert.True(sentEndOfMessage);

        var responseJson = Encoding.UTF8.GetString(sentBytes);
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
        Assert.Equal("pong", responseObj.GetProperty("action").GetString());
    }

    [Fact]
    public async Task ProcessMessage_GameplayAction_AddsToActionManager()
    {
        // Arrange
        var gameAction = new GameAction
        {
            Action = Actions.Move,
            TargetX = 5,
            TargetY = 10,
            Priority = 2
        };

        SetupWebSocketWithMessage(JsonSerializer.Serialize(gameAction));
        var client = CreateClient();

        // Act
        await client.StartAsync(CancellationToken.None);

        // Assert
        _actionManagerMock.Verify(m => m.AddAction(It.Is<GameAction>(a =>
            a.Action == Actions.Move &&
            a.TargetX == 5 &&
            a.TargetY == 10 &&
            a.Priority == 2)), Times.Once);
    }

    [Fact]
    public async Task ProcessMessage_InvalidJson_LogsWarning()
    {
        // Arrange
        SetupWebSocketWithMessage("this is not valid json");
        var client = CreateClient();

        // Act
        await client.StartAsync(CancellationToken.None);

        // Assert
        _loggerMock.Verify(logger => logger.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Can't parse this")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    // Helper methods
    private WebSocketConnectionHandler CreateClient()
    {
        return new WebSocketConnectionHandler(
            _gameCommandFactoryMock.Object,
            _loggerMock.Object,
            _httpContextAccessorMock.Object,
            _connectionManagerMock.Object);
    }

    private void SetupWebSocketToReturnCloseMessage()
    {
        var closeResult = new WebSocketReceiveResult(0, WebSocketMessageType.Close, true,
            WebSocketCloseStatus.NormalClosure, "Closing");

        _webSocketMock.Setup(w => w.State).Returns(WebSocketState.Open).Callback(() =>
            _webSocketMock.Setup(w => w.State).Returns(WebSocketState.Closed));

        _webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(closeResult);
    }

    private void SetupWebSocketWithMessage(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        // First call returns the message, second call returns close
        var messageResult = new WebSocketReceiveResult(messageBytes.Length, WebSocketMessageType.Text, true);
        var closeResult = new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);

        var callCount = 0;
        _webSocketMock.Setup(w => w.State).Returns(WebSocketState.Open);

        _webSocketMock.Setup(w => w.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
            .Callback<ArraySegment<byte>, CancellationToken>((buffer, token) =>
            {
                if (callCount == 0)
                {
                    messageBytes.CopyTo(buffer.Array, buffer.Offset);
                }
                callCount++;
            })
            .ReturnsAsync(() => callCount == 1 ? messageResult : closeResult);
    }

    private string CreateJsonMessage(Actions action)
    {
        var gameAction = new GameAction { Action = action };
        return JsonSerializer.Serialize(gameAction);
    }
}

// Helper class for testing WebSockets
public class TestWebSocketManager : WebSocketManager
{
    private readonly WebSocket _webSocket;

    public TestWebSocketManager(WebSocket webSocket)
    {
        _webSocket = webSocket;
    }

    public override Task<WebSocket> AcceptWebSocketAsync(string subProtocol)
    {
        return Task.FromResult(_webSocket);
    }

    public override bool IsWebSocketRequest => true;

    public override IList<string> WebSocketRequestedProtocols => throw new NotImplementedException();
}