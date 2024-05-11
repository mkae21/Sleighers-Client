using UnityEngine;

public struct ServerInfo
{
    public string ip;
    public int port;
}
public struct GoogleInfo
{
    public string id;
    public string secret;
}

public class SecretLoader : MonoBehaviour
{
#region PrivateVariables
    private const string ingamePath = "Json/InGameServer";
    private const string outgamePath = "Json/OutGameServer";
    private const string googlePath = "Json/GoogleAuth";
#endregion

#region PublicVariables
    public static SecretLoader instance;
    public static ServerInfo ingameServer;
    public static ServerInfo outgameServer;
    public static GoogleInfo googleAuth;
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this.gameObject);
        DontDestroyOnLoad(this.gameObject);

        ingameServer = LoadJson<ServerInfo>(ingamePath);
        outgameServer = LoadJson<ServerInfo>(outgamePath);
        googleAuth = LoadJson<GoogleInfo>(googlePath);
    }
    private T LoadJson<T>(string path)
    {
        TextAsset jsonString = Resources.Load<TextAsset>(path);
        return JsonUtility.FromJson<T>(jsonString.ToString());
    }
#endregion
}