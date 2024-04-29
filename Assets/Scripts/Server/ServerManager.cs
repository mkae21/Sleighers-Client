using UnityEngine;
using System.Net.Sockets;

public class ServerManager : MonoBehaviour
{
#region PrivateVariables
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnect = false;
    private static ServerManager instance = null;
    private string serverIP = string.Empty;
    private int serverPort = 0;
#endregion

#region PublicVariables
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        Init();
    }
    private void Init()
    {
        serverIP = SecretLoader.s_serverIp;
        serverPort = SecretLoader.s_serverPort;

        client = new TcpClient(serverIP, serverPort);
        if (client.Connected)
        {
            Debug.LogFormat("[ServerManager] 서버 접속 성공 {0}:{1}", serverIP, serverPort);
            stream = client.GetStream();
            isConnect = true;
        }
        else
        {
            Debug.LogError("[ServerManager] 서버 접속 실패");
            isConnect = false;
        }
    }
#endregion

#region PublicMethod
    public static ServerManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("[ServerManager] 인스턴스가 존재하지 않습니다.");
            return null;
        }

        return instance;
    }
    public void SendMessage(byte[] message)
    {
        if (isConnect)
        {
            Debug.LogFormat("[ServerManager] Send Message : {0}", message.Length);
            stream.Write(message, 0, message.Length);
        }
    }
    public NetworkStream GetStream()
    {
        return stream;
    }
    public TcpClient GetTcpClient()
    {
        return client;
    }
    public void CloseTcpClient()
    {
        client.Close();
    }
    public bool GetIsConnect()
    {
        return isConnect;
    }
    public void SetIsConnect(bool _isConnect)
    {
        isConnect = _isConnect;
    }
#endregion

}
