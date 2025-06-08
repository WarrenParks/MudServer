# Game Design Document: Multiplayer Turn-Based 2D Game

## Overview

A multiplayer, turn-based game where players connect via WebSockets. The game world is a 2D map. Each turn, players submit their actions (move, attack, etc.). At the end of each turn (e.g., every minute), the server processes all actions and updates the game state.

---

## Core Concepts

- **Multiplayer:** Players connect and interact in real-time using WebSockets.
- **2D Map:** The world is represented as a grid of tiles (walkable, walls, etc.).
- **Turn-Based:** Players submit actions during a turn window. All actions are resolved together at the end of the turn.
- **Action Resolution:** Actions are processed in a specific order (e.g., by priority or randomly).

---

## Gameplay Loop

Each turn is divided into distinct phases:

1. **Turn Start Phase**

   - The server broadcasts the current game state (map, player positions, etc.) to all players.
   - Players receive updated information and prepare for the new turn.

2. **Action Submission Phase**

   - Players choose and submit their actions (move, attack, defend, use item, etc.) via WebSocket.
   - There is a time window (e.g., 1 minute) for all players to submit their actions.

3. **Action Resolution Phase**

   - After the submission window closes, the server processes all received actions.
   - Actions are resolved in a specific order (by priority, randomly, or another rule).
   - The game state is updated based on the results of the actions.

4. **Turn End Phase**
   - The server broadcasts the results of the turn to all players.
   - Any end-of-turn effects or clean-up is handled.
   - The next turn begins, repeating the loop.

---

## Map

- **Grid-based:** Each tile has properties (walkable, terrain type, etc.).
- **Visibility:** (Optional) Players may have limited vision of the map.

---

## Actions

- **Move:** Change position on the map.
- **Attack:** Target another player or entity.
- **Defend:** Reduce damage or avoid attacks.
- **Use Item:** Apply an item effect.

---

## Networking

- **WebSocket Protocol:** All communication between client and server.
- **Message Types:** Join, submit action, broadcast state, etc.

---

## Future Ideas

- Items, power-ups, or abilities.
- Fog of war or hidden information.
- Player chat or messaging.
- Persistent world or sessions.

---

## References

- [Models/Map.cs](../Models/Map.cs)
- [Models/GameAction.cs](../Models/GameAction.cs)
- [Models/Turn.cs](../Models/Turn.cs)
- [Client.cs](../Client.cs)
- [Program.cs](../Program.cs)

---
