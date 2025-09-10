using Nakama;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Server Controller", order = 51)]
public class ServerController : ScriptableObject
{
    public IClient Client { get; private set; }
    public ISession Session { get; private set; }
    public IApiAccount Account { get; private set; }
    public ISocket Socket { get; private set; }

    public async Task<bool> Authenticate(string nickname)
    {
        Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        try
        {
            Session = await Client.AuthenticateCustomAsync($"customkey-{nickname}", username: nickname);
            PlayerPrefs.SetString("username", nickname);
            Debug.Log($"ServerController - Authenticate, user authenticated with username: {nickname}");
            Account = await Client.GetAccountAsync(Session);
            await ConnectSocket();
            return true;
        }
        catch (ApiResponseException e)
        {
            Debug.LogError($"ServerController - Authenticate, authentication failed with status code: {e.StatusCode}\n{e}");
            return false;
        }
    }

    public async Task ConnectSocket()
    {
        bool useMainThread = true;
        Socket = Client.NewSocket(useMainThread);

        bool appearOnline = true;
        int connectionTimeout = 30;
        try
        {
            await Socket.ConnectAsync(Session, appearOnline, connectionTimeout);
            Socket.Closed += OnSocketClosed;
        }
        catch (ApiResponseException ex)
        {
            Debug.LogError($"ServerController - ConnectSocket, failed to connect socket: {ex}");
        }
    }

    public async Task<IEnumerable<IApiMatch>> GetMatchesList()
    {
        try
        {
            int minPlayers = 0;
            int maxPlayers = 2;
            int limit = 10;
            bool authoritative = true;
            var result = await Client.ListMatchesAsync(Session, minPlayers, maxPlayers, limit, authoritative, null, null);
            Debug.Log($"ServerController - GetMatchesList, match list contains {result.Matches.Count()} matches");
            foreach (var match in result.Matches)
                Debug.Log($"{match.MatchId}: {match.Size}/2 players");

            return result.Matches;
        }
        catch (ApiResponseException ex)
        {
            Debug.LogError($"ServerController - GetMatchesList, failed to list matches: {ex}");
            return null;
        }
    }

    public async Task<CreateMatchResponseModel> CreateMatch()
    {
        try
        {
            var result = await Client.RpcAsync(Session, "create_match");
            var matchData = JsonUtility.FromJson<CreateMatchResponseModel>(result.Payload);
            Debug.Log($"ServerController - CreateMatch, match created with code: {matchData.code}, id: {matchData.matchId}");
            return matchData;
        }
        catch (ApiResponseException ex)
        {
            Debug.LogError($"ServerController - CreateMatch, failed to create match: {ex}");
            return null;
        }
    }

    public async Task<IMatch> JoinMatchWithId(string matchId)
    {
        try
        {
            var match = await Socket.JoinMatchAsync(matchId);
            Debug.Log($"ServerController - JoinMatchWithId, joined match with id: {matchId}");
            return match;
        }
        catch (ApiResponseException ex)
        {
            Debug.LogError($"ServerController - JoinMatchWithId, failed to join match {matchId}: {ex}");
            return null;
        }
    }

    public async Task<IMatch> JoinMatchWithCode(string matchCode)
    {
        try
        {
            var payload = new JoinMatchRequestModel() { code = matchCode };
            var result = await Client.RpcAsync(Session, id: "join_match", payload: JsonUtility.ToJson(payload));
            Debug.Log($"ServerController - JoinMatchWithCode, sent join_match rpc, result payload: {result.Payload}");
            string matchId = JsonUtility.FromJson<JoinMatchResponseModel>(result.Payload).matchId;
            var match = await Socket.JoinMatchAsync(matchId);
            Debug.Log($"ServerController - JoinMatchWithCode, joined match with code: {matchCode}");
            return match;
        }
        catch (ApiResponseException ex)
        {
            Debug.LogError($"ServerController - JoinMatchWithCode, failed to join match {matchCode}: {ex}");
            return null;
        }
    }

    public async Task LeaveMatch(string matchId)
    {
        try
        {
            await Socket.LeaveMatchAsync(matchId);
            Debug.Log($"ServerController - LeaveMatch, left the match {matchId}");
        }
        catch (ApiResponseException ex)
        {
            Debug.LogError($"ServerController - LeaveMatch, failed to leave match {matchId}: {ex}");
        }
    }

    private void OnSocketClosed()
    {
        Debug.LogWarning($"Socket Closed!");
    }

    private void OnDestroy()
    {
        Socket.CloseAsync();
    }
}

[Serializable]
public class CreateMatchResponseModel
{
    public string code;
    public string matchId;
}

[Serializable]
public class JoinMatchRequestModel
{
    public string code;
}

[Serializable]
public class JoinMatchResponseModel
{
    public string matchId;
}