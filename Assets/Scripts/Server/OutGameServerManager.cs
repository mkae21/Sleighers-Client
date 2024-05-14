using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class OutGameServerManager : MonoBehaviour
{
#region PrivateVariables
    private string serverIP = string.Empty;
    private int serverPort = 0;
    private SocketIOUnity socket;

    [SerializeField]
    private TMP_InputField idInputField;
    [SerializeField]
    private TMP_InputField pwInputField;
    [SerializeField]
    private TMP_InputField idSignUpField;
    [SerializeField]
    private TMP_InputField pwSignUpField;
    [SerializeField]
    private TMP_InputField nameSignUpField;

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
        GameManager.MatchMaking += MatchMaking;
    }

    private void Init()
    {
        // serverIP = "localhost"; // ë¡œì»¬ ?…Œ?Š¤?Š¸ ?š©
        serverIP = SecretLoader.outgameServer.ip;
        serverPort = SecretLoader.outgameServer.port;
        socket = new SocketIOUnity("http://" + serverIP +":"+serverPort);

        socket.OnConnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] ?„œë²? ? ‘?† ?„±ê³? {0}:{1}", serverIP, serverPort);
        };

        // ?—°ê²? ?•´? œ ?´ë²¤íŠ¸ ?•¸?“¤?Ÿ¬
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] ?„œë²? ? ‘?† ?•´? œ {0}:{1}", serverIP, serverPort);
        };

        // ?—?Ÿ¬ ?´ë²¤íŠ¸ ?•¸?“¤?Ÿ¬
        socket.OnError += (sender, e) =>
        {
            Debug.LogError("[OutGameServerManager] ?—?Ÿ¬ : " + e);
        };

        // ë¡œê·¸?¸ ?‘?‹µ ?´ë²¤íŠ¸ ?•¸?“¤?Ÿ¬
        socket.On("loginSucc", (res) =>
        {
            Debug.Log("Login success: " + res);
            DefaultLoginSucc();
        });

        socket.On("loginFail", (res) =>
        {
            Debug.Log("Login fail: " + res);
        });

        // ?šŒ?›ê°??ž… ?‘?‹µ ?´ë²¤íŠ¸ ?•¸?“¤?Ÿ¬
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
            UserData.instance.id = userInfo.id;
            UserData.instance.nickName = userInfo.name;
            UserData.instance.cart = userInfo.cart;
            UserData.instance.email = userInfo.email;
            Debug.Log("inquiryPlayer: " + userInfo.name);
            Debug.Log("inquiryPlayer: " + userInfo.cart);
            Debug.Log("inquiryPlayer: " + userInfo.email);
        });

        socket.On("setNameSucc", (res) =>
        {
            Debug.Log(res);
        });

        socket.On("setNameFail", (res) =>
        {
            Debug.Log(res);
        });


        socket.On("enterRoomFail", (res) =>
        {
            Debug.Log("enterRoomFail: " + res);
        });

        socket.On("enterRoomSucc", (res) =>
        {
            Debug.Log("endterRoomSucc:" + res);
        });

        socket.On("moveInGameScene", (res) =>
        {
            Debug.Log(res);
        });


        // ?„œë²? ?˜¤?”ˆ
        socket.Connect();

    }
#endregion

#region PublicMethod
    public static OutGameServerManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("[OutGameServerManager] ?¸?Š¤?„´?Š¤ê°? ì¡´ìž¬?•˜ì§? ?•Š?Šµ?‹ˆ?‹¤.");
            return null;
        }

        return instance;
    }

    public void LoginSucc(string email)
    {
        LoginInfo sendPacket = new LoginInfo();
        sendPacket.email = email;
        Debug.Log("ë³´ë‚¸?‹¤."+sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("loginSucc", jsonData);
    }

    public void DefaultLogin()
    {
        DefaultLoginInfo sendPacket = new DefaultLoginInfo();
        sendPacket.email = idInputField.text;
        sendPacket.password = pwInputField.text;
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("login", jsonData);
    }

    public void DefaultLoginSucc()
    {
        UnityThread.executeInLateUpdate (() =>
        {
            OutGameUI.instance.SuccLoginPanel();
        });
    }

    public void Signup()
    {
        SignupInfo sendPacket = new SignupInfo();
        sendPacket.email = idSignUpField.text;
        sendPacket.password = pwSignUpField.text;
        sendPacket.name = nameSignUpField.text;
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("signup", jsonData);
    }

    public void MatchMaking()
    {
        Packet sendPacket = new Packet();
        sendPacket.id = UserData.instance.id;
        Debug.Log("matchmaking id ë³´ë‚¸?‹¤."+sendPacket.id);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("matching", jsonData);
        OutGameUI.instance.MatchMakingUI();
    }

    public void SetName()
    {
        SetNameInfo sendPacket = new SetNameInfo();
        sendPacket.id = UserData.instance.id;
        sendPacket.name = OutGameUI.instance.settingNameField.text;
        OutGameUI.instance.settingNameField.text = "";
        Debug.Log("setName ë³´ë‚¸?‹¤." + sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("setName", jsonData);
    }

#endregion
}