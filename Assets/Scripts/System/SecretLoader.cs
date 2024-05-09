using UnityEngine;

struct ServerInfo
{
    public string ip;
    public string port;
}

public class SecretLoader : MonoBehaviour
{
#region PrivateVariables
    private const string path = "Json/secret";
    private TextAsset jsonString;
#endregion

#region PublicVariables
    public static string s_serverIp = "localhost";
    public static int s_serverPort = 5000;
#endregion

#region PrivateMethod
    private void Start()
    {
        jsonString = Resources.Load<TextAsset>(path);
        ServerInfo serverInfo = JsonUtility.FromJson<ServerInfo>(jsonString.ToString());
        s_serverIp = serverInfo.ip;
        s_serverPort = int.Parse(serverInfo.port);
    }
#endregion
}