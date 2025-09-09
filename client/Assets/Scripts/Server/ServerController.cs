using Nakama;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(menuName = "Game Data/Server Controller", order = 51)]
public class ServerController : ScriptableObject
{
    public IClient Client { get; private set; }
    public ISession Session { get; private set; }
    public IApiAccount Account { get; private set; }

    public async Task<bool> Authenticate(string nickname)
    {
        Client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        try
        {
            Session = await Client.AuthenticateCustomAsync($"customkey-{nickname}", username: nickname);
            Account = await Client.GetAccountAsync(Session);
            Debug.Log($"ServerController - Authenticate, user authenticated with username: {nickname}");
            PlayerPrefs.SetString("username", nickname);
            return true;
        }
        catch (ApiResponseException e)
        {
            Debug.LogError($"ServerController - Authenticate, authentication failed with status code: {e.StatusCode}\n{e}");
            return false;
        }
    }
}