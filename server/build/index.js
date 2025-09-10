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
    return {
        state: { presences: {} },
        tickRate: 1,
        label: ""
    };
}
;
function matchJoin(ctx, logger, nk, dispatcher, tick, state, presences) {
    presences.forEach(function (presence) {
        state.presences[presence.userId] = presence;
        logger.debug('%q joined Lobby match', presence.userId);
    });
    return {
        state: state
    };
}
function matchJoinAttempt(ctx, logger, nk, dispatcher, tick, state, presence, metadata) {
    logger.debug('%q attempted to join Lobby match', ctx.userId);
    return {
        state: state,
        accept: true
    };
}
function matchLeave(ctx, logger, nk, dispatcher, tick, state, presences) {
    presences.forEach(function (presence) {
        state.presences[presence.userId] = presence;
        logger.debug('%q left Lobby match', presence.userId);
    });
    return {
        state: state
    };
}
function matchLoop(ctx, logger, nk, dispatcher, tick, state, messages) {
    return {
        state: state
    };
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
    return {
        state: state
    };
}
var COLLECTION = "rooms";
var CODE_SIZE = 4;
var ADMIN_ID = "00000000-0000-0000-0000-000000000000";
var rpcCreateMatch = function (ctx, logger, nk, payload) {
    logger.info("rpcCreateMatch called by user %s", ctx.userId);
    var code = generateCode();
    var matchId = nk.matchCreate(MatchModuleName, {});
    var record = {
        collection: COLLECTION,
        key: code,
        userId: ADMIN_ID,
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
    var objects = nk.storageRead([{ collection: COLLECTION, key: code, userId: ADMIN_ID }]);
    if (objects.length === 0) {
        try {
            nk.matchGet(code);
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
    return JSON.stringify({ matchId: matchId });
};
function generateCode() {
    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var code = "";
    for (var i = 0; i < CODE_SIZE; i++) {
        code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return code;
}
function cleanupRoom(nk, logger, code) {
    try {
        nk.storageDelete([{ collection: COLLECTION, key: code, userId: ADMIN_ID }]);
        logger.info("Deleted room entry for code %s", code);
    }
    catch (err) {
        logger.error("Failed to delete room %s: %s", code, err);
    }
}
