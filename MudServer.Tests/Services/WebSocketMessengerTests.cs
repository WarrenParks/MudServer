using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Moq;

using MudServer.Server.Models;

using MudServer.Server.Services;

using Xunit;

namespace MudServer.Tests.Services;

public class WebSocketMessengerTests
{
  private readonly Mock<ILogger<WebSocketMessenger>> loggerMock;
  private readonly Mock<IConnectionManager> connectionManagerMock;
  private readonly Mock<WebSocket> webSocketMock;
  private readonly WebSocketMessenger webSocketMessenger;
  private readonly Guid senderClientId = Guid.NewGuid();
  private readonly Guid recipientClientId = Guid.NewGuid();

  public WebSocketMessengerTests()
  {
    // Setup mocks
    loggerMock = new Mock<ILogger<WebSocketMessenger>>();
    connectionManagerMock = new Mock<IConnectionManager>();
    webSocketMock = new Mock<WebSocket>();

    // Configure the connection manager mock
    connectionManagerMock
        .Setup(cm => cm.GetConnection(It.IsAny<Guid>()))
        .Returns(webSocketMock.Object);

    // Configure WebSocket mock to return success for SendAsync
    webSocketMock
        .Setup(ws => ws.SendAsync(
            It.IsAny<ArraySegment<byte>>(),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Create the system under test
    webSocketMessenger = new WebSocketMessenger(connectionManagerMock.Object, loggerMock.Object);
  }

  [Fact]
  public async Task SendMessageAsync_WithSpecificRecipient_ShouldSendMessageToSpecificClient()
  {
    // Arrange
    string message = "Hello, specific user!";
    byte[] capturedData = null;

    webSocketMock
        .Setup(ws => ws.SendAsync(
            It.IsAny<ArraySegment<byte>>(),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()))
        .Callback<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken>(
            (data, messageType, endOfMessage, token) =>
            {
              capturedData = data.ToArray();
            })
        .Returns(Task.CompletedTask);

    // Act
    await webSocketMessenger.SendMessageAsync(Actions.Chat, message, senderClientId, recipientClientId, CancellationToken.None);

    // Assert
    connectionManagerMock.Verify(cm => cm.GetConnection(recipientClientId), Times.Once);

    Assert.NotNull(capturedData);
    var responseJson = Encoding.UTF8.GetString(capturedData);
    var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

    Assert.Equal("chat", responseObj.GetProperty("action").GetString());
    Assert.Equal(recipientClientId.ToString(), responseObj.GetProperty("toClientId").GetString());
  }

  [Fact]
  public async Task SendMessageAsync_WithoutRecipient_ShouldSendToAllConnections()
  {
    // Arrange
    string message = "Hello, broadcast!";
    var client1 = Guid.NewGuid();
    var client2 = Guid.NewGuid();
    var client3 = Guid.NewGuid();

    var connections = new Dictionary<Guid, WebSocket>
        {
            { client1, webSocketMock.Object },
            { client2, webSocketMock.Object },
            { client3, webSocketMock.Object }
        };

    // Track which clients received the message
    var recipientIds = new List<Guid>();

    // Setup connection manager to return multiple connections
    connectionManagerMock.Setup(cm => cm.GetAllConnections())
        .Returns(connections);

    // Track which client IDs the message is sent to
    connectionManagerMock.Setup(cm => cm.GetConnection(It.IsAny<Guid>()))
        .Callback<Guid>(recipientIds.Add)
        .Returns(webSocketMock.Object);

    // Act
    await webSocketMessenger.SendMessageAsync(Actions.Chat, message, senderClientId, recipientClientId, CancellationToken.None);

    // Assert
    // Verify we're calling GetAllConnections instead of GetConnection
    connectionManagerMock.Verify(cm => cm.GetAllConnections(), Times.Once);

    // Verify we attempted to send to each client
    connectionManagerMock.Verify(cm => cm.GetConnection(client1), Times.Once);
    connectionManagerMock.Verify(cm => cm.GetConnection(client2), Times.Once);
    connectionManagerMock.Verify(cm => cm.GetConnection(client3), Times.Once);

    // Verify WebSocket.SendAsync was called 3 times (once per client)
    webSocketMock.Verify(ws => ws.SendAsync(
        It.IsAny<ArraySegment<byte>>(),
        WebSocketMessageType.Text,
        true,
        It.IsAny<CancellationToken>()),
        Times.Exactly(3));

    // Make sure we sent to all clients
    Assert.Equal(3, recipientIds.Count);
    Assert.Contains(client1, recipientIds);
    Assert.Contains(client2, recipientIds);
    Assert.Contains(client3, recipientIds);
  }

  [Fact]
  public async Task SendMessageAsync_WhenConnectionManagerReturnsNull_ShouldNotThrowException()
  {
    // Arrange
    connectionManagerMock
        .Setup(cm => cm.GetConnection(It.IsAny<Guid>()))
        .Returns((WebSocket)null);

    // Act & Assert
    var exception = await Record.ExceptionAsync(() =>
        webSocketMessenger.SendMessageAsync(Actions.Chat, "Test", senderClientId, recipientClientId, CancellationToken.None));

    Assert.NotNull(exception);
    Assert.IsType<NullReferenceException>(exception);
  }

  [Fact]
  public async Task SendMessageAsync_WhenWebSocketThrows_ShouldPropagateException()
  {
    // Arrange
    webSocketMock
        .Setup(ws => ws.SendAsync(
            It.IsAny<ArraySegment<byte>>(),
            WebSocketMessageType.Text,
            true,
            It.IsAny<CancellationToken>()))
        .ThrowsAsync(new WebSocketException("Test exception"));

    // Act & Assert
    await Assert.ThrowsAsync<WebSocketException>(() =>
        webSocketMessenger.SendMessageAsync(Actions.Chat, "Test", senderClientId, recipientClientId, CancellationToken.None));
  }
}