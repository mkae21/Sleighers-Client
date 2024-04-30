using UnityEngine;
using System.Net.Sockets;
using System.Collections.Generic;
using Protocol;

/* ServeManager.cs
 * - 서버와의 통신을 관리
 */
public class ServerManager : MonoBehaviour
{
#region PrivateVariables
    private TcpClient client;
    private NetworkStream stream;
    private bool isConnect = false;
    private static ServerManager instance = null;
    private string serverIP = string.Empty;
    private int serverPort = 0;
    private bool isHost = false;                // 호스트 여부 (서버에서 설정한 SuperGamer 정보를 가져옴)
    private Queue<KeyMessage> localQueue = null;   // 호스트에서 로컬로 처리하는 패킷을 쌓아두는 큐 (로컬처리하는 데이터는 서버로 발송 안함)

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
    private void Update()
    {
        if (isConnect && localQueue != null)
        {
            while (localQueue.Count > 0)
            {
                var msg = localQueue.Dequeue();
                WorldManager.instance.OnRecieveForLocal(msg);
            }
        
        }
    }
    private void Init()
    {
        // serverIP = SecretLoader.s_serverIp;
        serverIP = "localhost";
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
    public bool IsHost()
    {
        return isHost;
    }
    // 서버로 데이터 전송
    public void SendDataToInGame<T>(T msg)
    {
        if (isConnect)
        {
            var byteArray = DataParser.DataToJsonData<T>(msg);
            stream.Write(byteArray, 0, byteArray.Length);
        }
    }
    // 로컬 큐에 메시지 추가
    public void AddMsgToLocalQueue(KeyMessage _msg)
    {
        if (isHost == false || localQueue == null)
        {
            return;
        }

        localQueue.Enqueue(_msg);
    }
#endregion

}
