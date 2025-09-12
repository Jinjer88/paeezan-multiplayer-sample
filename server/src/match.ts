function matchInit(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, params: { [key: string]: string }): { state: nkruntime.MatchState, tickRate: number, label: string } {
    logger.debug('Lobby match created');

    const config = loadConfig(logger, nk);
    if (!config) {
        throw new Error("Failed to load game config");
    }

    const state: MatchState = { presences: {}, ready: {}, gameStarted: false, gameConfig: config };
    return { state, tickRate: 4, label: "1v1" };
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
        state.presences[presence.userId] = presence;
        logger.debug('%q left Lobby match', presence.userId);
    });

    if (presences.length === 0) {
        logger.debug('No players left in the match, terminating.');
        return null;
    }

    return { state };
}

function matchLoop(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, messages: nkruntime.MatchMessage[]): { state: nkruntime.MatchState } | null {
    // logger.debug('Lobby match loop executed');

    if (!state.gameStarted)
        return { state };

    for (const m of messages) {
        if (m.opCode === 2) {
            state.ready[m.sender.userId] = true;
            logger.info("Player %s ready", m.sender.userId);
        }
    }

    if (!state.started) {
        var allReady = true;
        for (var userId in state.presences) {
            if (!state.ready[userId]) {
                allReady = false;
                break;
            }
        }

        if (!allReady) {
            return { state };
        }

        logger.info("All players ready, starting game");
        state.started = true;
        dispatcher.broadcastMessage(3, JSON.stringify({ type: "start_match", countdown: 3 }));
    }

    return { state };
}

function matchSignal(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, data: string): { state: nkruntime.MatchState, data?: string } | null {
    logger.debug('Lobby match signal received: ' + data);

    return {
        state,
        data: "Lobby match signal received: " + data
    };
}

function matchTerminate(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, graceSeconds: number): { state: nkruntime.MatchState } | null {
    logger.debug('Lobby match terminated');

    return { state };
}

function loadConfig(logger: nkruntime.Logger, nk: nkruntime.Nakama): GameConfig | undefined {
    try {
        const raw = nk.fileRead("./game-config.json");
        return JSON.parse(raw) as GameConfig;
    } catch (e) {
        logger.error("Failed to load config: %s", e);
        return undefined;
    }
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

interface MatchState {
    presences: { [userId: string]: nkruntime.Presence };
    ready: { [userId: string]: boolean };
    gameStarted: boolean;
    gameConfig: GameConfig;
}