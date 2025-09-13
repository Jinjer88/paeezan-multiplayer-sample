function matchInit(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: { [key: string]: string }): { state: nkruntime.MatchState, tickRate: number, label: string } {
    logger.debug('Lobby match created');

    const config = loadConfig(logger, nk);
    if (!config) {
        throw new Error("Failed to load game config");
    }

    const state: GameState = { presences: {}, ready: {}, gameStarted: false, gameConfig: config, units: [], towers: {}, host: params.host, manas: {}, meleeCooldowns: {}, rangedCooldowns: {} };
    logger.debug('Match state created, host: %s', state.host);
    return { state, tickRate: 5, label: "1v1" };
};

function matchJoin(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[]): { state: nkruntime.MatchState } | null {
    presences.forEach(function (presence) {
        state.presences[presence.userId] = presence;
        state.ready[presence.userId] = false;
        dispatcher.broadcastMessage(0, JSON.stringify({ type: "match_config", config: state.gameConfig }), [presence]);
        logger.debug('%q joined Lobby match, config sent', presence.userId);
    });

    const playerNames = [];
    for (var userId in state.presences) {
        playerNames.push(state.presences[userId].username);
    }
    dispatcher.broadcastMessage(4, JSON.stringify({
        type: "lobby_update",
        players: playerNames
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

function matchLoop(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, messages: nkruntime.MatchMessage[]): { state: nkruntime.MatchState } | null {
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
                if (data.unitType) {
                    const unit: Unit = {
                        position: m.sender.userId === state.host ? -5 : 5,
                        health: state.gameConfig.units[data.unitType].health,
                        attackTimer: 0,
                        type: data.unitType,
                        owner: m.sender.userId,
                    };
                    state.units.push(unit);
                    dispatcher.broadcastMessage(6, JSON.stringify({ type: "new_unit", unit }));
                    logger.info("New unit added: %s by %s", data.unitType, m.sender.username);
                }
            }
        }
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

function loadConfig(logger: nkruntime.Logger, nk: nkruntime.Nakama): GameConfig | null {
    try {
        const raw = nk.fileRead("./game-config.json");
        return JSON.parse(raw) as GameConfig;
    } catch (e) {
        logger.error("Failed to load config: %s", e);
        return null;
    }
}

function allReady(state: nkruntime.MatchState): boolean {
    for (const userId in state.ready) {
        if (!state.ready[userId]) {
            return false;
        }
    }
    return true;
}

interface GameConfig {
    towers: {
        health: number;
        attack: number;
    };
    units: {
        melee: { health: number; attack: number; cost: number; speed: number; range: number; };
        ranged: { health: number; attack: number; cost: number; speed: number; range: number; };
    };
}

interface Unit {
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
    host?: string;
    gameConfig: GameConfig;
}