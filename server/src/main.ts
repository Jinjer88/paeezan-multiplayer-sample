const MatchModuleName = "match_module";

const InitModule: nkruntime.InitModule = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    logger.info('starting module initialization. InitModule');

    initializer.registerRpc('create_match', rpcCreateMatch);
    initializer.registerRpc('join_match', rpcJoinMatch);

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