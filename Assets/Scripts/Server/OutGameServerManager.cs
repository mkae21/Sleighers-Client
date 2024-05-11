using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using System.IO;
using UnityEditor.PackageManager;
using SocketIOClient;
using UnityEngine.SceneManagement;

public class OutGameServerManager : MonoBehaviour
{
#region PrivateVariables
    private string serverIP = string.Empty;
    private int serverPort = 0;
    private SocketIOUnity socket;

    [SerializeField]
    private GameObject AuthPanel;
    [SerializeField]
    private GameObject LobbyPanel;
    [SerializeField]
    private GameObject TopBar;
    [SerializeField]
    private GameObject idInputField;

#endregion

#region PublicVariables
    public static OutGameServerManager instance = null;
#endregion


#region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        // serverIP = "localhost"; // 로컬 테스트 용
        serverIP = SecretLoader.outgameServer.ip;
        serverPort = SecretLoader.outgameServer.port;
        socket = new SocketIOUnity("http://" + serverIP +":"+serverPort);

        socket.OnConnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] 서버 접속 성공 {0}:{1}", serverIP, serverPort);
        };

        // 연결 해제 이벤트 핸들러
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] 서버 접속 해제 {0}:{1}", serverIP, serverPort);
        };

        // 에러 이벤트 핸들러
        socket.OnError += (sender, e) =>
        {
            Debug.LogError("[OutGameServerManager] 에러 : " + e);
        };

        // 로그인 응답 이벤트 핸들러
        socket.On("loginSucc", (res) =>
        {
            Debug.Log("Login success: " + res);
            //SceneManager.LoadScene("Topdown");
            DefaltLoginSucc();
        });

        socket.On("loginFail", (res) =>
        {
            Debug.Log("Login fail: " + res);
        });

        // 회원가입 응답 이벤트 핸들러
        socket.On("signupSucc", (res) =>
        {
            Debug.Log("Signup success: " + res);
        });

        socket.On("signupFail", (res) =>
        {
            Debug.Log("Signup fail: " + res);
        });

        socket.On("inquiryPlayer", (res) =>
        {
            Debug.Log("inquiryPlayer: " + res);
            string jsonString = res.GetValue<string>();
            UserInfo userInfo = JsonUtility.FromJson<UserInfo>(jsonString);
            Debug.Log("inquiryPlayer: " + userInfo);
            Debug.Log("inquiryPlayer: " + userInfo.name);
            Debug.Log("inquiryPlayer: " + userInfo.cart);
            Debug.Log("inquiryPlayer: " + userInfo.email);
        });

        socket.On("enterRoomFail", (res) =>
        {
            Debug.Log(res);
        });

        socket.On("enterRoomSucc", (res) =>
        {
            Debug.Log(res);
        });

        socket.On("moveInGameScene", (res) =>
        {
            Debug.Log(res);
        });

        socket.On("enterRoomSucc", (res) =>
        {
            Debug.Log(res);
        });

        // 서버 연결
        socket.Connect();

    }
#endregion

#region PublicMethod
    public static OutGameServerManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("[OutGameServerManager] 인스턴스가 존재하지 않습니다.");
            return null;
        }

        return instance;
    }

    public void LoginSucc(string email)
    {
        Packet sendPacket = new Packet();
        sendPacket.email = email;
        Debug.Log("보낸다."+sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("loginSucc", jsonData);
    }

    public void DefaltLogin()
    {
        Packet sendPacket = new Packet();
        sendPacket.email = "test";
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("loginSucc", jsonData);
    }

    public void DefaltLoginSucc()
    {
        AuthPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        TopBar.SetActive(true);
    }
#endregion
}