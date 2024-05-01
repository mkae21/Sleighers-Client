using UnityEngine;
using System.Net.Sockets;
using System.Collections.Generic;
using Protocol;
using System.Collections;

/* ServeManager.cs
 * - 서버와의 통신을 관리
 */
public class ServerManager : MonoBehaviour
{
#region PrivateVariables
    private IEnumerator ServerPollCoroutine;
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
    [SerializeField] private bool isHost = false;                // 호스트 여부 (서버에서 설정한 SuperGamer 정보를 가져옴)
    private Queue<KeyMessage> localQueue = null;   // 호스트에서 로컬로 처리하는 패킷을 쌓아두는 큐 (로컬처리하는 데이터는 서버로 발송 안함)

#endregion

#region PublicVariables
#endregion

#region PrivateMethod
    private void Awake()
    {
        ServerPollCoroutine = Poll();
        if (instance != null)
            Destroy(instance);
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        Init();
        StartCoroutine(ServerPollCoroutine);
    }
    private void Update()
    {
        if (IsConnect && localQueue != null)
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
        // serverIP = "localhost"; // 로컬 테스트 용
        serverIP = SecretLoader.s_serverIp;
        serverPort = SecretLoader.s_serverPort;

        Client = new TcpClient(serverIP, serverPort);
        if (Client.Connected)
        {
            Debug.LogFormat("[ServerManager] 서버 접속 성공 {0}:{1}", serverIP, serverPort);
            Stream = Client.GetStream();
            IsConnect = true;
        }
        else
        {
            Debug.LogError("[ServerManager] 서버 접속 실패");
            IsConnect = false;
        }
    }
    private IEnumerator Poll()
    {
        while (true)
        {
            if (Stream.DataAvailable && IsConnect)
                WorldManager.instance.OnReceive();
            yield return new WaitForSeconds(0.1f);
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
    public bool SetHostSession(int _id)
    {
        if (WorldManager.instance.MyPlayerId == _id)
        {
            isHost = true;
            localQueue = new Queue<KeyMessage>();
        }
        else
        {
            isHost = false;
            localQueue = null;
        }
        Debug.LogFormat("[ServerManager] 호스트 여부 : {0}", isHost);
        return true;
    }
    public void SetSubHost(int hostSessionId)
    {
        // Debug.Log("서브 호스트 세션 설정 진입");
        // // 누가 서브 호스트 세션인지 서버에서 보낸 정보값 확인
        // // 서버에서 보낸 SuperGamer 정보로 GameRecords의 SuperGamer 정보 갱신
        // foreach (var record in gameRecords)
        // {
        //     if (record.Value.m_sessionId.Equals(hostSessionId))
        //     {
        //         record.Value.m_isSuperGamer = true;
        //     }
        //     else
        //     {
        //         record.Value.m_isSuperGamer = false;
        //     }
        // }
        // // 내가 호스트 세션인지 확인
        // if (hostSessionId.Equals(Backend.Match.GetMySessionId()))
        // {
        //     isHost = true;
        // }
        // else
        // {
        //     isHost = false;
        // }

        // hostSession = hostSessionId;

        // Debug.Log("서브 호스트 여부 : " + isHost);
        // // 호스트 세션이면 로컬에서 처리하는 패킷이 있으므로 로컬 큐를 생성해준다
        // if (isHost)
        // {
        //     localQueue = new Queue<KeyMessage>();
        // }
        // else
        // {
        //     localQueue = null;
        // }

        // Debug.Log("서브 호스트 설정 완료");
    }
    public void CloseStream()
    {
        Stream.Close();
    }
    public void CloseTcpClient()
    {
        Client.Close();
    }
    public bool IsHost()
    {
        return isHost;
    }
    // 서버로 데이터 전송
    public void SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        Stream.Write(byteArray, 0, byteArray.Length);
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
