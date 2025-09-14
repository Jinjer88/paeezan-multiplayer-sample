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
var tickRate = 5;
function matchInit(ctx, logger, nk, params) {
    logger.debug('Lobby match created');
    var config = loadConfig(logger, nk);
    if (!config) {
        throw new Error("Failed to load game config");
    }
    var state = { presences: {}, ready: {}, gameStarted: false, gameConfig: config, units: [], towers: {}, host: params.host, manas: {}, meleeCooldowns: {}, rangedCooldowns: {} };
    logger.debug('Match state created, host: %s', state.host);
    return { state: state, tickRate: tickRate, label: "1v1" };
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
    var playerIDs = [];
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
    return { state: state };
}
function matchJoinAttempt(ctx, logger, nk, dispatcher, tick, state, presence, metadata) {
    logger.debug('%q attempted to join Lobby match', ctx.userId);
    return { state: state, accept: true };
}
function matchLeave(ctx, logger, nk, dispatcher, tick, state, presences) {
    presences.forEach(function (presence) {
        delete state.presences[presence.userId];
        logger.debug('%q left the match', presence.username);
    });
    if (Object.keys(state.presences).length === 0) {
        logger.debug('All players left the match, terminating');
        return null;
    }
    return { state: state };
}
function matchSignal(ctx, logger, nk, dispatcher, tick, state, data) {
    logger.debug('Lobby match signal received: ' + data);
    return { state: state, data: "Lobby match signal received: " + data };
}
function matchTerminate(ctx, logger, nk, dispatcher, tick, state, graceSeconds) {
    logger.debug('Lobby match terminated');
    return { state: state };
}
function matchLoop(ctx, logger, nk, dispatcher, tick, state, messages) {
    if (state.winner) {
        return { state: state };
    }
    for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
        var m = messages_1[_i];
        if (m.opCode === 2) {
            state.ready[m.sender.userId] = true;
            logger.info("Player %s ready", m.sender.username);
        }
    }
    if (!state.gameStarted && Object.keys(state.presences).length === 2 && allReady(state)) {
        logger.info("All players ready, starting game");
        state.gameStarted = true;
        for (var userId in state.presences) {
            state.manas[userId] = 10;
            state.towers[userId] = state.gameConfig.towers.health;
            state.meleeCooldowns[userId] = 0;
            state.rangedCooldowns[userId] = 0;
        }
        dispatcher.broadcastMessage(3, JSON.stringify({ type: "start_match", countdown: 3 }));
    }
    if (state.gameStarted) {
        for (var _a = 0, messages_2 = messages; _a < messages_2.length; _a++) {
            var m = messages_2[_a];
            if (m.opCode === 5) {
                var data = JSON.parse(nk.binaryToString(m.data));
                spawnUnit(data, state, m, dispatcher, logger);
            }
        }
        updatePlayersMana(state);
        updateUnits(state, dispatcher);
    }
    return { state: state };
}
function loadConfig(logger, nk) {
    try {
        var raw = nk.fileRead("./game-config.json");
        return JSON.parse(raw);
    }
    catch (e) {
        logger.error("Failed to load config: %s", e);
        return null;
    }
}
function updatePlayersMana(state) {
    for (var userId in state.manas) {
        if (state.manas[userId] < 10) {
            state.manas[userId] += state.gameConfig.manaRegenRate / tickRate;
        }
    }
}
function updateUnits(state, dispatcher) {
    var gameState = state;
    var updates = [];
    for (var i = 0; i < gameState.units.length; i++) {
        var unit = gameState.units[i];
        if (unit.health <= 0) {
            continue;
        }
        var config = gameState.gameConfig.units[unit.type];
        var speed = config.moveSpeed;
        var range = config.attackRange;
        var damage = config.attackDamage;
        var updated = false;
        var enemyTowerPos = unit.owner === gameState.host ? 5 : -5;
        var towerOwner = unit.owner === gameState.host ? getOpponentId(unit.owner, gameState) : gameState.host;
        var enemyInRange = false;
        for (var j = 0; j < gameState.units.length; j++) {
            var other = gameState.units[j];
            if (other.owner !== unit.owner && other.health > 0) {
                var dist = Math.abs(unit.position - other.position);
                var inFront = (unit.owner === gameState.host) ? other.position > unit.position : other.position < unit.position;
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
                        gameState.winner = unit.owner;
                        dispatcher.broadcastMessage(10, JSON.stringify({ type: "game_over", winner: gameState.winner }));
                        return;
                    }
                }
                enemyInRange = true;
            }
        }
        if (!enemyInRange) {
            if (unit.owner === gameState.host && unit.position < 5 - range) {
                unit.position += speed / tickRate;
                updated = true;
            }
            else if (unit.owner !== gameState.host && unit.position > -5 + range) {
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
        dispatcher.broadcastMessage(7, JSON.stringify({ type: "unit_positions", updates: updates }));
    }
}
function getOpponentId(userId, state) {
    for (var id in state.presences) {
        if (id !== userId)
            return id;
    }
    return "";
}
function allReady(state) {
    for (var userId in state.ready) {
        if (!state.ready[userId]) {
            return false;
        }
    }
    return true;
}
function spawnUnit(data, state, m, dispatcher, logger) {
    if (data.unitType) {
        var unitCost = state.gameConfig.units[data.unitType].cost;
        if (state.manas[m.sender.userId] < unitCost) {
            logger.info("Player %s tried to spawn %s but has insufficient mana", m.sender.username, data.unitType);
            return;
        }
        var unit = {
            id: state.units.length,
            position: m.sender.userId === state.host ? -5 : 5,
            health: state.gameConfig.units[data.unitType].health,
            attackTimer: 0,
            type: data.unitType,
            owner: m.sender.userId,
        };
        state.manas[m.sender.userId] -= unitCost;
        state.units.push(unit);
        dispatcher.broadcastMessage(6, JSON.stringify({ type: "new_unit", unit: unit }));
        logger.info("New unit added: %s by %s", data.unitType, m.sender.username);
    }
}
var Collection = "rooms";
var CodeSize = 4;
var SystemID = "00000000-0000-0000-0000-000000000000";
var rpcCreateMatch = function (ctx, logger, nk, payload) {
    logger.info("rpcCreateMatch called by user %s", ctx.userId);
    var code = generateCode();
    var matchId = nk.matchCreate(MatchModuleName, { "host": ctx.userId });
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
