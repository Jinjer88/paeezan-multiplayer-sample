const tickRate = 5;

function matchInit(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: { [key: string]: string }): { state: nkruntime.MatchState, tickRate: number, label: string } {
    logger.debug('Lobby match created');

    const config = loadConfig(logger, nk);
    if (!config) {
        throw new Error("Failed to load game config");
    }

    const state: GameState = { presences: {}, ready: {}, gameStarted: false, gameConfig: config, units: [], towers: {}, host: params.host, manas: {}, meleeCooldowns: {}, rangedCooldowns: {} };
    logger.debug('Match state created, host: %s', state.host);
    return { state, tickRate: tickRate, label: "1v1" };
};

function matchJoin(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {
    presences.forEach(function (presence) {
        state.presences[presence.userId] = presence;
        state.ready[presence.userId] = false;
        dispatcher.broadcastMessage(0, JSON.stringify({ type: "match_config", config: state.gameConfig }), [presence]);
        logger.debug('%q joined Lobby match, config sent', presence.userId);
    });

    const playerNames = [];
    const playerIDs = [];
    for (var userId in state.presences) {
        playerNames.push(state.presences[userId].username);
        playerIDs.push(userId);
    }
    dispatcher.broadcastMessage(4, JSON.stringify({
        type: "lobby_update",
        playerNames: playerNames,
        playerIDs: playerIDs
    }));

    if (Object.keys(state.presences).length === 2) {
        dispatcher.broadcastMessage(1, JSON.stringify({ type: "match_ready" }));
    }

    return { state };
}

function matchJoinAttempt(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presence: nkruntime.Presence, metadata: { [key: string]: any }): { state: nkruntime.MatchState, accept: boolean, rejectMessage?: string | undefined } | null {
    logger.debug('%q attempted to join Lobby match', ctx.userId);

    return { state, accept: true };
}

function matchLeave(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {
    presences.forEach(function (presence) {
        delete state.presences[presence.userId];
        logger.debug('%q left the match', presence.username);
    });

    if (Object.keys(state.presences).length === 0) {
        logger.debug('All players left the match, terminating');
        return null;
    }

    return { state };
}

function matchSignal(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, data: string): { state: nkruntime.MatchState, data?: string } | null {
    logger.debug('Lobby match signal received: ' + data);

    return { state, data: "Lobby match signal received: " + data };
}

function matchTerminate(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, graceSeconds: number): { state: nkruntime.MatchState } | null {
    logger.debug('Lobby match terminated');

    return { state };
}

function matchLoop(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, messages: nkruntime.MatchMessage[]): { state: nkruntime.MatchState } | null {
    if (state.winner) {
        return { state };
    }

    for (const m of messages) {
        if (m.opCode === 2) {
            state.ready[m.sender.userId] = true;
            logger.info("Player %s ready", m.sender.username);
        }
    }

    if (!state.gameStarted && Object.keys(state.presences).length === 2 && allReady(state)) {
        logger.info("All players ready, starting game");
        state.gameStarted = true;
        for (const userId in state.presences) {
            state.manas[userId] = 10;
            state.towers[userId] = state.gameConfig.towers.health;
            state.meleeCooldowns[userId] = 0;
            state.rangedCooldowns[userId] = 0;
        }
        dispatcher.broadcastMessage(3, JSON.stringify({ type: "start_match", countdown: 3 }));
    }

    if (state.gameStarted) {
        for (const m of messages) {
            if (m.opCode === 5) {
                const data = JSON.parse(nk.binaryToString(m.data));
                spawnUnit(data, state, m, dispatcher, logger);
            }
        }

        updatePlayersMana(state);
        updateUnits(logger, nk, state, dispatcher);
    }

    return { state };
}

function loadConfig(logger: nkruntime.Logger, nk: nkruntime.Nakama): GameConfig | null {
    try {
        const raw = nk.fileRead("./game-config.json");
        logger.debug("Game config loaded");
        return JSON.parse(raw) as GameConfig;
    } catch (e) {
        logger.error("Failed to load config: %s", e);
        return null;
    }
}

function updatePlayersMana(state: nkruntime.MatchState) {
    for (const userId in state.manas) {
        if (state.manas[userId] < 10) {
            state.manas[userId] += state.gameConfig.manaRegenRate / tickRate;
        }
    }
}

function updateUnits(logger: nkruntime.Logger, nk: nkruntime.Nakama, state: nkruntime.MatchState, dispatcher: nkruntime.MatchDispatcher) {
    const gameState = state as unknown as GameState;
    const updates: { id: number, position: number, health?: number }[] = [];

    for (let i = 0; i < gameState.units.length; i++) {
        const unit = gameState.units[i];

        if (unit.health <= 0) {
            continue;
        }

        const config = gameState.gameConfig.units[unit.type];
        const speed = config.moveSpeed;
        const range = config.attackRange;
        const damage = config.attackDamage;

        let updated = false;
        let enemyTowerPos = unit.owner === gameState.host ? 5 : -5;
        let towerOwner = unit.owner === gameState.host ? getOpponentId(unit.owner, gameState) : gameState.host;
        let enemyInRange = false;

        // Check enemy units
        for (let j = 0; j < gameState.units.length; j++) {
            const other = gameState.units[j];
            if (other.owner !== unit.owner && other.health > 0) {
                const dist = Math.abs(unit.position - other.position);
                const inFront = (unit.owner === gameState.host) ? other.position > unit.position : other.position < unit.position;

                if (dist <= range && inFront) {
                    enemyInRange = true;
                    if (unit.attackTimer <= 0) {
                        other.health -= damage;
                        unit.attackTimer = config.attackSpeed;
                        dispatcher.broadcastMessage(9, JSON.stringify({
                            type: "unit_attack",
                            attacker: unit.id,
                            target: other.id,
                            damage: damage,
                            targetHealth: other.health
                        }));

                        if (other.health <= 0) {
                            dispatcher.broadcastMessage(11, JSON.stringify({ type: "unit_dead", unitId: other.id }));
                        }
                    }
                    break;
                }
            }
        }

        // If no enemy unit in range, check tower
        if (!enemyInRange) {
            if (Math.abs(unit.position - enemyTowerPos) <= range) {
                if (unit.attackTimer <= 0) {
                    gameState.towers[towerOwner] -= damage;
                    unit.attackTimer = config.attackSpeed;
                    dispatcher.broadcastMessage(8, JSON.stringify({
                        type: "tower_attack",
                        unitId: unit.id,
                        attacker: unit.owner,
                        damage: damage,
                        towerOwner: towerOwner,
                        towerHealth: gameState.towers[towerOwner]
                    }));

                    if (gameState.towers[towerOwner] <= 0) {
                        logger.info("Player %s wins!", unit.owner);
                        writeLeaderbaordRecord(logger, nk, unit.owner, gameState.presences[unit.owner].username);
                        gameState.winner = unit.owner;
                        dispatcher.broadcastMessage(10, JSON.stringify({ type: "game_over", winner: gameState.winner }));
                        return;
                    }
                }
                enemyInRange = true;
            }
        }

        // If no enemy in range, move forward
        if (!enemyInRange) {
            if (unit.owner === gameState.host && unit.position < 5 - range) {
                unit.position += speed / tickRate;
                updated = true;
            } else if (unit.owner !== gameState.host && unit.position > -5 + range) {
                unit.position -= speed / tickRate;
                updated = true;
            }
        }

        if (unit.attackTimer > 0) {
            unit.attackTimer -= 1 / tickRate;
        }

        if (updated || unit.health <= 0) {
            updates.push({
                id: unit.id,
                position: unit.position,
                health: unit.health
            });
        }
    }

    if (updates.length > 0) {
        dispatcher.broadcastMessage(7, JSON.stringify({ type: "unit_positions", updates }));
    }
}

function getOpponentId(userId: string, state: GameState): string {
    for (const id in state.presences) {
        if (id !== userId) return id;
    }
    return "";
}

function allReady(state: nkruntime.MatchState): boolean {
    for (const userId in state.ready) {
        if (!state.ready[userId]) {
            return false;
        }
    }
    return true;
}

function spawnUnit(data: any, state: nkruntime.MatchState, m: nkruntime.MatchMessage, dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger) {
    if (data.unitType) {
        const unitCost = state.gameConfig.units[data.unitType].cost;
        if (state.manas[m.sender.userId] < unitCost) {
            logger.info("Player %s tried to spawn %s but has insufficient mana", m.sender.username, data.unitType);
            return;
        }

        const unit: Unit = {
            id: state.units.length,
            position: m.sender.userId === state.host ? -5 : 5,
            health: state.gameConfig.units[data.unitType].health,
            attackTimer: 0,
            type: data.unitType,
            owner: m.sender.userId,
        };

        state.manas[m.sender.userId] -= unitCost;
        state.units.push(unit);
        dispatcher.broadcastMessage(6, JSON.stringify({ type: "new_unit", unit }));
        logger.info("New unit added: %s by %s", data.unitType, m.sender.username);
    }
}

function writeLeaderbaordRecord(logger: nkruntime.Logger, nk: nkruntime.Nakama, userId: string, username: string) {
    try {
        nk.leaderboardRecordWrite(winsLeaderboardId, userId, username, 1);
    } catch (error) {
        logger.error("Error writing leaderboard record: " + error);
    }
}

interface GameConfig {
    manaRegenRate: number;
    towers: {
        health: number;
        attack: number;
    };
    units: {
        melee: UnitConfig;
        ranged: UnitConfig;
    };
}

interface Unit {
    id: number;
    position: number;
    health: number;
    attackTimer: number;
    type: "melee" | "ranged";
    owner: string;
}

interface GameState {
    presences: { [userId: string]: nkruntime.Presence };
    ready: { [userId: string]: boolean };
    gameStarted: boolean;
    units: Unit[];
    towers: { [userId: string]: number };
    manas: { [userId: string]: number };
    meleeCooldowns: { [userId: string]: number };
    rangedCooldowns: { [userId: string]: number };
    host: string;
    gameConfig: GameConfig;
    winner?: string;
}

interface UnitConfig {
    health: number;
    attackDamage: number;
    attackSpeed: number;
    attackRange: number;
    cost: number;
    moveSpeed: number;
    cooldown: number;
}
