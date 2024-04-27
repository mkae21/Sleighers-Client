using UnityEngine;
using System.IO;

public class Secret
{
    public string server_ip;
    public string server_port;
}

public class SecretLoader : MonoBehaviour
{
#region PrivateVariables
    private string secret_path = "Assets/secret.json";
    private string json_string;
#endregion

#region PublicVariables
    public static string server_ip;
    public static int server_port;
#endregion

#region PrivateMethod
    private void Start()
    {
        json_string = File.ReadAllText(secret_path);
        Secret secret = JsonUtility.FromJson<Secret>(json_string);

        server_ip = secret.server_ip;
        server_port = int.Parse(secret.server_port);

        Debug.LogFormat("Secret Loader: Server IP: {0}, Server Port: {1}", server_ip, server_port);
    }
#endregion
}