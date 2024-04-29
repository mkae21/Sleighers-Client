using UnityEngine;
using System.IO;

public class Secret
{
    public string m_serverIp;
    public string m_serverPort;
}

public class SecretLoader : MonoBehaviour
{
#region PrivateVariables
    private string secretPath = "Assets/secret.json";
    private string jsonString;
#endregion

#region PublicVariables
    public static string s_serverIp = "localhost";
    public static int s_serverPort = 30303;
#endregion

#region PrivateMethod
    private void Start()
    {
        jsonString = File.ReadAllText(secretPath);
        Secret secret = JsonUtility.FromJson<Secret>(jsonString);
        s_serverIp = secret.m_serverIp;
        s_serverPort = int.Parse(secret.m_serverPort);

        Debug.LogFormat("[SecretLoader] {0}:{1}", s_serverIp, s_serverPort);
    }
    #endregion
}