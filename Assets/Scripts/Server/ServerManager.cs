using UnityEngine;
using System.Net.Sockets;
using System.Collections;
using System.Threading.Tasks;

/* ServeManager.cs
 * - 서버와의 통신을 관리
 */
public class ServerManager : MonoBehaviour
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
        LogManager.instance.Log("[ServerManager] Start()");
        Init();
    }
    private void FixedUpdate()
    {
        if (Stream.DataAvailable && IsConnect)
            WorldManager.instance.Polling();
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
            LogManager.instance.Log("[ServerManager] 서버 접속 성공 " + serverIP + ":" + serverPort.ToString());
            Stream = Client.GetStream();
            IsConnect = true;
        }
        else
        {
            Debug.LogError("[ServerManager] 서버 접속 실패");
            LogManager.instance.Log("[ServerManager] 서버 접속 실패 " + serverIP + ":" + serverPort.ToString());
            IsConnect = false;
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
    
    public void CloseStream()
    {
        Stream.Close();
    }
    public void CloseTcpClient()
    {
        Client.Close();
    }
    // 서버로 데이터 전송
    public async Task SendDataToInGame<T>(T msg)
    {
        var byteArray = DataParser.DataToJsonData<T>(msg);
        await Stream.WriteAsync(byteArray, 0, byteArray.Length);
    }
#endregion
}
