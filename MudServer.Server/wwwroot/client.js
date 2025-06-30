// Client configuration
const config = {
  reconnectInterval: 2000, // ms
  mapSize: 10, // 10x10 grid
};

// Client state
const state = {
  connected: false,
  username: `user_${Math.floor(Math.random() * 1000)}`,
  clientId: null,
  onlineUsers: new Set(),
  playerPosition: { x: 5, y: 5 }, // Default position in the middle
};

// DOM Elements
const elements = {
  connectionIndicator: document.getElementById("connection-indicator"),
  connectionText: document.getElementById("connection-text"),
  chatMessages: document.getElementById("chat-messages"),
  eventLog: document.getElementById("event-log"),
  chatInput: document.getElementById("chat-input"),
  sendButton: document.getElementById("send-chat"),
  onlineUsers: document.getElementById("online-users"),
  gameMap: document.getElementById("game-map"),
  tabButtons: document.querySelectorAll(".tab-button"),
  tabPanes: document.querySelectorAll(".tab-pane"),
  moveButtons: {
    north: document.getElementById("move-north"),
    east: document.getElementById("move-east"),
    south: document.getElementById("move-south"),
    west: document.getElementById("move-west"),
  },
};

// WebSocket connection
let ws = null;

// Initialize the client
function init() {
  connectWebSocket();
  initMap();
  bindEvents();
  initTabs();
}

// Connect to WebSocket server
function connectWebSocket() {
  const protocol = window.location.protocol === "https:" ? "wss:" : "ws:";
  const wsUrl = `${protocol}//${window.location.host}`;

  ws = new WebSocket(wsUrl);

  ws.onopen = handleSocketOpen;
  ws.onmessage = handleSocketMessage;
  ws.onclose = handleSocketClose;
  ws.onerror = handleSocketError;
}

// Handle WebSocket open event
function handleSocketOpen() {
  state.connected = true;
  updateConnectionStatus(true);
  addSystemMessage("Connected to server");

  // Send initial register message
  sendJson({
    action: "register",
    username: state.username,
    key: "default-key", // Replace with actual key if needed
  });
}

// Handle WebSocket messages
function handleSocketMessage(event) {
  try {
    const data = JSON.parse(event.data);
    processMessage(data);
  } catch (error) {
    console.error("Failed to parse message:", error);
    addEventMessage("Received invalid message format");
  }
}

// Process incoming WebSocket message
function processMessage(data) {
  console.log("Received message:", data);

  switch (data.action.toLowerCase()) {
    case "welcome":
      handleWelcomeMessage(data);
      break;

    case "chat":
      handleChatMessage(data);
      break;

    case "userjoined":
      handleUserJoined(data);
      break;

    case "userleft":
      handleUserLeft(data);
      break;

    case "updatemap":
      handleMapUpdate(data);
      break;

    case "notification":
      handleNotification(data);
      break;

    case "gameevent":
      handleGameEvent(data);
      break;

    default:
      addEventMessage(`Unhandled message type: ${data.action}`);
  }
}

// Handle WebSocket close event
function handleSocketClose() {
  state.connected = false;
  updateConnectionStatus(false);
  addSystemMessage("Disconnected from server");

  // Attempt to reconnect
  setTimeout(connectWebSocket, config.reconnectInterval);
}

// Handle WebSocket error
function handleSocketError(error) {
  console.error("WebSocket error:", error);
  addSystemMessage("Connection error");
}

// Send a JSON message through WebSocket
function sendJson(data) {
  if (ws && ws.readyState === WebSocket.OPEN) {
    ws.send(JSON.stringify(data));
  } else {
    addSystemMessage("Cannot send message: not connected");
  }
}

// Update the connection status indicator
function updateConnectionStatus(isConnected) {
  elements.connectionIndicator.className = isConnected ? "connected" : "disconnected";
  elements.connectionText.textContent = isConnected ? "Connected" : "Disconnected";
}

// Add a system message to chat
function addSystemMessage(message) {
  const messageElement = document.createElement("div");
  messageElement.className = "message system-message";
  messageElement.textContent = message;
  elements.chatMessages.appendChild(messageElement);
  elements.chatMessages.scrollTop = elements.chatMessages.scrollHeight;
}

// Add a chat message
function addChatMessage(clientId, username, message) {
  const messageElement = document.createElement("div");
  messageElement.className = "message user-message";

  const usernameSpan = document.createElement("span");
  usernameSpan.className = "username";
  //usernameSpan.textContent = `${username} (${clientId}): `;
  usernameSpan.textContent = `${username || clientId}: `;

  messageElement.appendChild(usernameSpan);
  messageElement.appendChild(document.createTextNode(message));

  elements.chatMessages.appendChild(messageElement);
  elements.chatMessages.scrollTop = elements.chatMessages.scrollHeight;
}

// Add an event message
function addEventMessage(message) {
  const messageElement = document.createElement("div");
  messageElement.className = "message event-message";
  messageElement.textContent = message;
  elements.eventLog.appendChild(messageElement);
  elements.eventLog.scrollTop = elements.eventLog.scrollHeight;
}

// Initialize the game map
function initMap() {
  elements.gameMap.innerHTML = "";

  for (let y = 0; y < config.mapSize; y++) {
    for (let x = 0; x < config.mapSize; x++) {
      const tile = document.createElement("div");
      tile.className = "map-tile grass";
      tile.dataset.x = x;
      tile.dataset.y = y;
      elements.gameMap.appendChild(tile);
    }
  }

  updatePlayerPosition();
}

// Update player position on the map
function updatePlayerPosition() {
  // Remove player marker from all tiles
  document.querySelectorAll(".player-tile").forEach((tile) => {
    tile.classList.remove("player-tile");
  });

  // Add player marker to current position
  const playerTile = document.querySelector(
    `.map-tile[data-x="${state.playerPosition.x}"][data-y="${state.playerPosition.y}"]`
  );
  if (playerTile) {
    playerTile.classList.add("player-tile");
  }
}

// Update online users list
function updateOnlineUsers() {
  elements.onlineUsers.innerHTML = "";

  Array.from(state.onlineUsers)
    .sort()
    .forEach((username) => {
      const userElement = document.createElement("li");
      userElement.className = "user-online";
      userElement.textContent = username;
      elements.onlineUsers.appendChild(userElement);
    });
}

// Bind event listeners
function bindEvents() {
  // Chat input
  elements.chatInput.addEventListener("keypress", (event) => {
    if (event.key === "Enter") {
      sendChatMessage();
    }
  });

  elements.sendButton.addEventListener("click", sendChatMessage);

  // Movement buttons
  elements.moveButtons.north.addEventListener("click", () => movePlayer(0, -1));
  elements.moveButtons.east.addEventListener("click", () => movePlayer(1, 0));
  elements.moveButtons.south.addEventListener("click", () => movePlayer(0, 1));
  elements.moveButtons.west.addEventListener("click", () => movePlayer(-1, 0));
}

// Initialize tab functionality
function initTabs() {
  elements.tabButtons.forEach((button) => {
    button.addEventListener("click", () => {
      // Remove active class from all buttons and panes
      elements.tabButtons.forEach((btn) => btn.classList.remove("active"));
      elements.tabPanes.forEach((pane) => pane.classList.remove("active"));

      // Add active class to clicked button and corresponding pane
      button.classList.add("active");
      const tabName = button.dataset.tab;
      document.getElementById(`${tabName}-tab`).classList.add("active");
    });
  });
}

// Send a chat message
function sendChatMessage() {
  const message = elements.chatInput.value.trim();
  if (message && state.connected) {
    sendJson({
      action: "chat",
      message: message,
    });
    addChatMessage("me", state.username, message);
    elements.chatInput.value = "";
  }
}

// Move player on the map
function movePlayer(deltaX, deltaY) {
  const newX = Math.max(0, Math.min(config.mapSize - 1, state.playerPosition.x + deltaX));
  const newY = Math.max(0, Math.min(config.mapSize - 1, state.playerPosition.y + deltaY));

  if (newX !== state.playerPosition.x || newY !== state.playerPosition.y) {
    sendJson({
      action: "move",
      targetX: newX,
      targetY: newY,
      priority: 1,
    });

    // Optimistic update (will be corrected if server rejects)
    state.playerPosition.x = newX;
    state.playerPosition.y = newY;
    updatePlayerPosition();
    addEventMessage(`Moved to (${newX}, ${newY})`);
  }
}

// Handle the welcome message
function handleWelcomeMessage(data) {
  state.clientId = data.clientId;
  addSystemMessage(`Welcome! You are connected as ${state.username}`);

  if (data.users) {
    state.onlineUsers = new Set(data.users);
    updateOnlineUsers();
  }
}

// Handle a chat message
function handleChatMessage(data) {
  addChatMessage(data.fromClientId, data.fromUser, data.message);
}

// Handle user joined notification
function handleUserJoined(data) {
  state.onlineUsers.add(data.username);
  updateOnlineUsers();
  addSystemMessage(`${data.username} has joined`);
}

// Handle user left notification
function handleUserLeft(data) {
  state.onlineUsers.delete(data.username);
  updateOnlineUsers();
  addSystemMessage(`${data.username} has left`);
}

// Handle map update
function handleMapUpdate(data) {
  if (data.playerPosition) {
    state.playerPosition = data.playerPosition;
    updatePlayerPosition();
  }

  // Update map tiles if provided
  if (data.tiles) {
    data.tiles.forEach((tile) => {
      const tileElement = document.querySelector(`.map-tile[data-x="${tile.x}"][data-y="${tile.y}"]`);
      if (tileElement) {
        // Remove all terrain classes
        tileElement.classList.remove("grass", "water", "wall");
        // Add the new terrain class
        tileElement.classList.add(tile.terrainType.toLowerCase());
      }
    });
  }
}

// Handle notification event
function handleNotification(data) {
  addEventMessage(data.message);
}

// Initialize the client when the page loads
window.addEventListener("load", init);
