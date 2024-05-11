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
using TMPro;

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
    private TMP_InputField idInputField;

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
        // serverIP = "localhost"; // 濡쒖뺄 �뀒�뒪�듃 �슜
        serverIP = SecretLoader.outgameServer.ip;
        serverPort = SecretLoader.outgameServer.port;
        socket = new SocketIOUnity("http://" + serverIP +":"+serverPort);

        socket.OnConnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] �꽌踰� �젒�냽 �꽦怨� {0}:{1}", serverIP, serverPort);
        };

        // �뿰寃� �빐�젣 �씠踰ㅽ듃 �빖�뱾�윭
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] �꽌踰� �젒�냽 �빐�젣 {0}:{1}", serverIP, serverPort);
        };

        // �뿉�윭 �씠踰ㅽ듃 �빖�뱾�윭
        socket.OnError += (sender, e) =>
        {
            Debug.LogError("[OutGameServerManager] �뿉�윭 : " + e);
        };

        // 濡쒓렇�씤 �쓳�떟 �씠踰ㅽ듃 �빖�뱾�윭
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

        // �쉶�썝媛��엯 �쓳�떟 �씠踰ㅽ듃 �빖�뱾�윭
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
            UserData.instance.id = userInfo.email.Split('@')[0];    // �씠硫붿씪�뿉�꽌 �븘�씠�뵒 異붿텧
            UserData.instance.nickName = userInfo.name;
            UserData.instance.cart = userInfo.cart;
            UserData.instance.email = userInfo.email;
            Debug.Log("inquiryPlayer: " + userInfo);
            Debug.Log("inquiryPlayer: " + userInfo.name);
            Debug.Log("inquiryPlayer: " + userInfo.cart);
            Debug.Log("inquiryPlayer: " + userInfo.email);

            DefaltLoginSucc();
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

        // �꽌踰� �뿰寃�
        socket.Connect();

    }
#endregion

#region PublicMethod
    public static OutGameServerManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("[OutGameServerManager] �씤�뒪�꽩�뒪媛� 議댁옱�븯吏� �븡�뒿�땲�떎.");
            return null;
        }

        return instance;
    }

    public void LoginSucc(string email)
    {
        LoginInfo sendPacket = new LoginInfo();
        sendPacket.email = email;
        Debug.Log("蹂대궦�떎."+sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("loginSucc", jsonData);
    }

    public void DefaltLogin()
    {
        Packet sendPacket = new Packet();
        sendPacket.email = idInputField.text;
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("login", jsonData);
    }

    public void DefaltLoginSucc()
    {
        AuthPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        TopBar.SetActive(true);
    }

    public void MatchMaking()
    {
        Packet sendPacket = new Packet();
        sendPacket.id = UserData.instance.id;
        Debug.Log("matchmaking id 蹂대궦�떎."+sendPacket.id);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("matching", jsonData);
    }
#endregion
}