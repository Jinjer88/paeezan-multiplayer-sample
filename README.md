# 1v1 PvP Prototype – Technical Interview Task

## Overview

This project is a **1v1 server-authoritative multiplayer prototype** inspired by Clash Royale. It was implemented with **Unity (C#)** for the client and **Nakama (TypeScript runtime)** for the server. The architecture ensures that all critical game logic (unit spawning, movement, combat, win conditions) is validated and simulated on the server, while the client is responsible for rendering and input.

### Features

* Player authentication with nickname entry.
* Lobby system: create/join match via a match code.
* Server-authoritative combat loop:

  * Units spawn, move, and attack under server control.
  * Towers are destroyed when health ≤ 0 → winner declared.
* Mana system with regeneration and unit costs.
* Bonus features: Unit vs Unit combat and global wins leaderboard.
* Game configuration stored in **`game-config.json`**, easily editable without server restart.
* Build available for **Windows** and **Android**.

---

## Architecture

* **Server**

  * Runs Nakama in Docker with CockroachDB.
  * TypeScript modules implement match handler, RPCs, and leaderboard.
  * Config-driven: `game-config.json` defines units, towers, and mana regen.

* **Client**

  * Unity 2022.3.62 LTS project (inside `/client`).
  * Handles menus, lobby UI, and rendering of the battle.
  * Connects to Nakama via WebSocket.
  * Displays server state (unit positions, attacks, game over).

---

## Folder Structure

```
/project-root
  /client   -> Unity project (C#)
  /server   -> Nakama server logic (TypeScript, docker-compose)
  README.md
  game-config.json
```

---

## Running the Server

1. Install [Docker](https://www.docker.com/products/docker-desktop/) (Docker Desktop on Windows).
2. Navigate to the `/server` folder in terminal.
3. Run:

   ```bash
   docker compose up -d
   ```
4. Nakama will start with CockroachDB and load the custom match logic automatically.

---

## Running the Client

### Windows Build

* Run `paeezan-multiplayer-sample.exe`

### Android Build

* Install the APK `PvPPrototype.apk`.
* Make sure the device is connected to the same network as the server so it can reach 127.0.0.1.

---

## How to Play

1. Enter a nickname and connect. The game saves your user data and will automatically authenticate you the next time you open it.

   * To run two instances locally for testing, click the **New User** button to return to the register page and pick a different nickname.
2. Choose to **Create Match** (you’ll receive a code) or **Join Match** (enter a code).
3. Once both players join, the match begins after a countdown.
4. Spend mana to deploy units. Units march toward the opponent’s tower, fighting enemy units if encountered.
5. First player to destroy the opponent’s tower wins.
6. Wins are recorded on the **Leaderboard** accessible from the main menu.

---

## Notes

* This is a prototype: networking is authoritative but simplified (no prediction/rollback).
* Unit behavior and game balance are fully editable via `game-config.json`.
* Leaderboard increments player wins globally and never resets.
