// JSON templates (could be loaded from a file in the future)
const jsonTemplates = {
  ChatAll: {
    action: "chat",
    message: "Hello, world!",
  },
  Move: {
    action: "move",
    priority: 1,
    targetX: 1,
    targetY: 2,
  },
  Attack: {
    action: "attack",
    priority: 1,
    target: "enemy-id",
  },
  "Start Game": {
    action: "startGame",
  },
  Ping: {
    action: "ping",
  },
};

const term = document.getElementById("terminal");
const templateList = document.getElementById("template-list");
const jsonEditor = document.getElementById("json-editor");
const sendJsonBtn = document.getElementById("send-json-btn");
let ws;
let inputBuffer = "";
let history = ["Welcome to the MUD Terminal!"];
let selectedTemplate = Object.keys(jsonTemplates)[0];

function renderTerminal() {
  // Show history, then prompt with current buffer
  term.innerHTML = history.join("<br>") + `<br>&gt; ${inputBuffer}`;
  term.scrollTop = term.scrollHeight;
}

function renderTemplateList() {
  templateList.innerHTML = "";
  Object.keys(jsonTemplates).forEach((name) => {
    const li = document.createElement("li");
    li.textContent = name;
    if (name === selectedTemplate) li.classList.add("selected");
    li.onclick = () => {
      selectedTemplate = name;
      jsonEditor.value = JSON.stringify(jsonTemplates[name], null, 2);
      renderTemplateList();
    };
    templateList.appendChild(li);
  });
}

function connect() {
  ws = new WebSocket((location.protocol === "https:" ? "wss://" : "ws://") + location.host);
  ws.onopen = () => {
    history.push("Connected to server");
    renderTerminal();
  };
  ws.onmessage = (e) => {
    history.push(e.data.replace(/\n/g, "<br>"));
    renderTerminal();
  };
  ws.onclose = () => {
    history.push("Disconnected");
    renderTerminal();
  };
}
connect();

term.addEventListener("keydown", function (e) {
  if (ws && ws.readyState === WebSocket.OPEN) {
    if (e.key === "Enter") {
      ws.send(inputBuffer);
      history.push(`&gt; ${inputBuffer}`);
      inputBuffer = "";
    } else if (e.key === "Backspace") {
      inputBuffer = inputBuffer.slice(0, -1);
    } else if (e.key.length === 1) {
      inputBuffer += e.key;
    }
    renderTerminal();
    e.preventDefault();
  }
});

// JSON editor logic
renderTemplateList();
jsonEditor.value = JSON.stringify(jsonTemplates[selectedTemplate], null, 2);

sendJsonBtn.onclick = function () {
  if (ws && ws.readyState === WebSocket.OPEN) {
    let json;
    try {
      json = JSON.parse(jsonEditor.value);
    } catch (e) {
      alert("Invalid JSON");
      return;
    }
    ws.send(JSON.stringify(json));
    history.push(`&gt; [JSON] ${JSON.stringify(json)}`);
    renderTerminal();
  }
};

renderTerminal();
term.focus();
