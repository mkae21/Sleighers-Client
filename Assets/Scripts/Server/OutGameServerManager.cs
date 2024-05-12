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
        // serverIP = "localhost"; // Î°úÏª¨ ?Öå?ä§?ä∏ ?ö©
        serverIP = SecretLoader.outgameServer.ip;
        serverPort = SecretLoader.outgameServer.port;
        socket = new SocketIOUnity("http://" + serverIP +":"+serverPort);

        socket.OnConnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] ?ÑúÎ≤? ?†ë?Üç ?Ñ±Í≥? {0}:{1}", serverIP, serverPort);
        };

        // ?ó∞Í≤? ?ï¥?†ú ?ù¥Î≤§Ìä∏ ?ï∏?ì§?ü¨
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogFormat("[OutGameServerManager] ?ÑúÎ≤? ?†ë?Üç ?ï¥?†ú {0}:{1}", serverIP, serverPort);
        };

        // ?óê?ü¨ ?ù¥Î≤§Ìä∏ ?ï∏?ì§?ü¨
        socket.OnError += (sender, e) =>
        {
            Debug.LogError("[OutGameServerManager] ?óê?ü¨ : " + e);
        };

        // Î°úÍ∑∏?ù∏ ?ùë?ãµ ?ù¥Î≤§Ìä∏ ?ï∏?ì§?ü¨
        socket.On("loginSucc", (res) =>
        {
            Debug.Log("Login success: " + res);
            //SceneManager.LoadScene("Topdown");
            DefaultLoginSucc();
        });

        socket.On("loginFail", (res) =>
        {
            Debug.Log("Login fail: " + res);
        });

        // ?öå?õêÍ∞??ûÖ ?ùë?ãµ ?ù¥Î≤§Ìä∏ ?ï∏?ì§?ü¨
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


        // ?ÑúÎ≤? ?ò§?îà
        socket.Connect();

    }
#endregion

#region PublicMethod
    public static OutGameServerManager Instance()
    {
        if (instance == null)
        {
            Debug.LogError("[OutGameServerManager] ?ù∏?ä§?Ñ¥?ä§Í∞? Ï°¥Ïû¨?ïòÏß? ?ïä?äµ?ãà?ã§.");
            return null;
        }

        return instance;
    }

    public void LoginSucc(string email)
    {
        LoginInfo sendPacket = new LoginInfo();
        sendPacket.email = email;
        Debug.Log("Î≥¥ÎÇ∏?ã§."+sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("loginSucc", jsonData);
    }

    public void DefaultLogin()
    {
        LoginSucc(idInputField.text);
    }

    public void DefaultLoginSucc()
    {
        Debug.Log("DefaultLoginSucc Start");
        OutGameUI.instance.panels[0].SetActive(false);  // auth panel
        OutGameUI.instance.panels[1].SetActive(true);   // lobby panel
        OutGameUI.instance.topBar.SetActive(true);
        Debug.Log("DefaultLoginSucc");
    }

    public void MatchMaking()
    {
        Packet sendPacket = new Packet();
        sendPacket.id = UserData.instance.id;
        Debug.Log("matchmaking id Î≥¥ÎÇ∏?ã§."+sendPacket.id);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("matching", jsonData);
    }

    public void SetName()
    {
        SetNameInfo sendPacket = new SetNameInfo();
        sendPacket.id = UserData.instance.id;
        sendPacket.name = OutGameUI.instance.settingNameField.text;
        OutGameUI.instance.settingNameField.text = "";
        Debug.Log("setName Î≥¥ÎÇ∏?ã§." + sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("setName", jsonData);
    }

#endregion
}