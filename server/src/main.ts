const MatchModuleName = "match_module";
const winsLeaderboardId = "lb_wins";

const InitModule: nkruntime.InitModule = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    logger.info('starting module initialization. InitModule');

    initializer.registerRpc('create_match', rpcCreateMatch);
    initializer.registerRpc('join_match', rpcJoinMatch);

    createLeaderboard(logger, nk);

    initializer.registerMatch(MatchModuleName, {
        matchInit,
        matchJoinAttempt,
        matchJoin,
        matchLeave,
        matchLoop,
        matchTerminate,
        matchSignal
    });
}

function createLeaderboard(logger: nkruntime.Logger, nk: nkruntime.Nakama) {
    let authoritative = true;
    let sort = nkruntime.SortOrder.DESCENDING;
    let operator = nkruntime.Operator.INCREMENTAL;
    try {
        nk.leaderboardCreate(winsLeaderboardId, authoritative, sort, operator);
        logger.info("Leaderboard created: " + winsLeaderboardId);
    } catch (error) {
        logger.error("Error creating leaderboard: " + error);
    }
}