"use strict";
var MatchModuleName = "match_module";
var InitModule = function (ctx, logger, nk, initializer) {
    logger.info('starting module initialization. InitModule');
    initializer.registerRpc('create_match', rpcCreateMatch);
    initializer.registerRpc('join_match', rpcJoinMatch);
    initializer.registerMatch(MatchModuleName, {
        matchInit: matchInit,
        matchJoinAttempt: matchJoinAttempt,
        matchJoin: matchJoin,
        matchLeave: matchLeave,
        matchLoop: matchLoop,
        matchTerminate: matchTerminate,
        matchSignal: matchSignal
    });
};
function matchInit(ctx, logger, nk, params) {
    logger.debug('Lobby match created');
    var config = loadConfig(logger, nk);
    if (!config) {
        throw new Error("Failed to load game config");
    }
    var state = { presences: {}, ready: {}, gameStarted: false, gameConfig: config };
    return { state: state, tickRate: 4, label: "1v1" };
}
;
function matchJoin(ctx, logger, nk, dispatcher, tick, state, presences) {
    presences.forEach(function (presence) {
        state.presences[presence.userId] = presence;
        state.ready[presence.userId] = false;
        dispatcher.broadcastMessage(0, JSON.stringify({ type: "match_config", config: state.gameConfig }), [presence]);
        logger.debug('%q joined Lobby match, config sent', presence.userId);
    });
    var playerNames = [];
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
    return { state: state };
}
function matchJoinAttempt(ctx, logger, nk, dispatcher, tick, state, presence, metadata) {
    logger.debug('%q attempted to join Lobby match', ctx.userId);
    return { state: state, accept: true };
}
function matchLeave(ctx, logger, nk, dispatcher, tick, state, presences) {
    presences.forEach(function (presence) {
        state.presences[presence.userId] = presence;
        logger.debug('%q left Lobby match', presence.userId);
    });
    if (presences.length === 0) {
        logger.debug('No players left in the match, terminating.');
        return null;
    }
    return { state: state };
}
function matchLoop(ctx, logger, nk, dispatcher, tick, state, messages) {
    if (!state.gameStarted)
        return { state: state };
    for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
        var m = messages_1[_i];
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
            return { state: state };
        }
        logger.info("All players ready, starting game");
        state.started = true;
        dispatcher.broadcastMessage(3, JSON.stringify({ type: "start_match", countdown: 3 }));
    }
    return { state: state };
}
function matchSignal(ctx, logger, nk, dispatcher, tick, state, data) {
    logger.debug('Lobby match signal received: ' + data);
    return {
        state: state,
        data: "Lobby match signal received: " + data
    };
}
function matchTerminate(ctx, logger, nk, dispatcher, tick, state, graceSeconds) {
    logger.debug('Lobby match terminated');
    return { state: state };
}
function loadConfig(logger, nk) {
    try {
        var raw = nk.fileRead("./game-config.json");
        return JSON.parse(raw);
    }
    catch (e) {
        logger.error("Failed to load config: %s", e);
        return undefined;
    }
}
var Collection = "rooms";
var CodeSize = 4;
var SystemID = "00000000-0000-0000-0000-000000000000";
var rpcCreateMatch = function (ctx, logger, nk, payload) {
    logger.info("rpcCreateMatch called by user %s", ctx.userId);
    var code = generateCode();
    var matchId = nk.matchCreate(MatchModuleName, {});
    var record = {
        collection: Collection,
        key: code,
        userId: SystemID,
        value: { matchId: matchId },
        permissionRead: 2,
        permissionWrite: 1
    };
    nk.storageWrite([record]);
    logger.info("Created room %s for match %s", code, matchId);
    return JSON.stringify({ code: code, matchId: matchId });
};
var rpcJoinMatch = function (ctx, logger, nk, payload) {
    var data = JSON.parse(payload);
    var code = data.code;
    var objects = nk.storageRead([{ collection: Collection, key: code, userId: SystemID }]);
    if (objects.length === 0) {
        try {
            var matchData = nk.matchGet(code);
            if (matchData && matchData.size > 1) {
                throw Error("Match is full");
            }
            logger.info("Code %s is a valid matchId", code);
            return JSON.stringify({ matchId: code });
        }
        catch (err) {
            logger.error("Invalid code %s: %s", code, err);
            throw Error("Invalid room code");
        }
    }
    var matchId = objects[0].value.matchId;
    logger.info("Player %s requested join for code %s -> match %s", ctx.userId, code, matchId);
    cleanupRoom(nk, logger, code);
    return JSON.stringify({ matchId: matchId });
};
function generateCode() {
    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var code = "";
    for (var i = 0; i < CodeSize; i++) {
        code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return code;
}
function cleanupRoom(nk, logger, code) {
    try {
        nk.storageDelete([{ collection: Collection, key: code, userId: SystemID }]);
        logger.info("Deleted room entry for code %s", code);
    }
    catch (err) {
        logger.error("Failed to delete room %s: %s", code, err);
    }
}
