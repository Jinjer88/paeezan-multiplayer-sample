const Collection = "rooms";
const CodeSize = 4;
const SystemID = "00000000-0000-0000-0000-000000000000";

let rpcCreateMatch: nkruntime.RpcFunction = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    logger.info("rpcCreateMatch called by user %s", ctx.userId);
    const code = generateCode();
    const matchId = nk.matchCreate(MatchModuleName, {});

    const record: nkruntime.StorageWriteRequest = {
        collection: Collection,
        key: code,
        userId: SystemID,
        value: { matchId },
        permissionRead: 2,
        permissionWrite: 1
    };

    nk.storageWrite([record]);

    logger.info("Created room %s for match %s", code, matchId);

    return JSON.stringify({ code, matchId });
}

let rpcJoinMatch: nkruntime.RpcFunction = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, payload: string): string {
    const data = JSON.parse(payload);
    const code = data.code;

    const objects = nk.storageRead([{ collection: Collection, key: code, userId: SystemID }]);
    if (objects.length === 0) {
        try {
            const matchData = nk.matchGet(code);
            if (matchData && matchData.size > 1) {
                throw Error("Match is full");
            }
            logger.info("Code %s is a valid matchId", code);
            return JSON.stringify({ matchId: code });
        } catch (err) {
            logger.error("Invalid code %s: %s", code, err);
            throw Error("Invalid room code");
        }
    }

    const matchId = objects[0].value.matchId;
    logger.info("Player %s requested join for code %s -> match %s", ctx.userId, code, matchId);

    cleanupRoom(nk, logger, code);
    return JSON.stringify({ matchId });
}

function generateCode(): string {
    const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    let code = "";
    for (let i = 0; i < CodeSize; i++) {
        code += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return code;
}

function cleanupRoom(nk: nkruntime.Nakama, logger: nkruntime.Logger, code: string): void {
    try {
        nk.storageDelete([{ collection: Collection, key: code, userId: SystemID }]);
        logger.info("Deleted room entry for code %s", code);
    } catch (err) {
        logger.error("Failed to delete room %s: %s", code, err);
    }
}