using UnityEngine;
using System.Net.Sockets;
using System;

/* InGameServeManager.cs
 * - 인게임 서버와의 통신을 관리
 */
public partial class ServerManager : MonoBehaviour
{
#region PrivateVariables
    private TcpClient client;
    public TcpClient Client
    {
        get 
        { 
            if (client == null)
            {
                Debug.LogWarningFormat("[ServerManager] Client가 null입니다.");
                return null;
            }
            return client;
        }
        set { client = value; }
    }
    private NetworkStream stream;
    public NetworkStream Stream
    {
        get
        {
            if (stream == null)
            {
                Debug.LogWarningFormat("[ServerManager] Stream이 null입니다.");
                return null;
            }
            return stream; 
        }
        set { stream = value; }
    }
    private bool isConnect = false;
    public bool IsConnect
    {
        get { return isConnect; }
        set { isConnect = value; }
    }
    private static ServerManager instance = null;
    private string serverIP = string.Empty;
    private int serverPort = 0;

#endregion

#region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(this.gameObject);
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        ConnectInGame();
    }
    private void FixedUpdate()
    {
        if (IsConnect && Stream.DataAvailable)
            WorldManager.instance.Polling();
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
    public void ConnectInGame()
    {
        // serverIP = "localhost"; // 로컬 테스트 용
        serverIP = SecretLoader.ingameServer.ip;
        serverPort = SecretLoader.ingameServer.port;

        try
        {
            Client = new TcpClient(serverIP, serverPort);
            Debug.LogFormat("[ServerManager] 인게임 서버 접속 성공 {0}:{1}", serverIP, serverPort);
            LogManager.instance.Log("[ServerManager] 인게임 서버 접속 성공 " + serverIP + ":" + serverPort.ToString());
            Stream = Client.GetStream();
            IsConnect = true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ServerManager] 인게임 서버 접속 실패: " + e.Message);
            LogManager.instance.Log("[ServerManager] 인게임 서버 접속 실패 " + serverIP + ":" + serverPort.ToString());
            IsConnect = false;
            return;
        }
    }
    public void DisconnectInGame()
    {
        if (IsConnect)
        {
            Stream.Close();
            Client.Close();
            IsConnect = false;
            Debug.LogFormat("[ServerManager] 인게임 서버 접속 해제 {0}:{1}", serverIP, serverPort);
        }
        else
            Debug.LogFormat("[ServerManager] 인게임 서버에 접속중이지 않습니다. {0}:{1}", serverIP, serverPort);
    }
    // 서버로 데이터 전송
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Stream.WriteAsync(byteArray, 0, byteArray.Length);
    }
#endregion
}
