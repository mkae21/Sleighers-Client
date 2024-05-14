using Newtonsoft.Json;
using System.Collections.Generic;
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
    public RoomData roomData;

#endregion

    #region PrivateMethod
    private void Awake()
    {
        if (instance != null)
            Destroy(instance);
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        Init();
        GameManager.MatchMaking += MatchMaking;
    }

    private void OnDestroy()
    {
        GameManager.MatchMaking -= MatchMaking;
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
            DefaultLoginSucc();
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

        });

        socket.On("setNameFail", (res) =>
        {

        });

        socket.On("enterRoomFail", (res) =>
        {
            Debug.Log("enterRoomFail: " + res);
        });

        socket.On("enterRoomSucc", (res) =>
        {
            UnityThread.executeInLateUpdate(() =>
            {
                Debug.Log("endterRoomSucc:" + res);
                roomData = ParseData(res.GetValue<string>());

                // 자신의 MatchInfo를 첫 번째 요소로 정렬
                roomData.playerList.Sort((a, b) =>
                {
                    if (a.id == UserData.instance.id)
                        return -1;
                    else if (b.id == UserData.instance.id)
                        return 1;
                    else
                        return 0;
                });
                Debug.Log(roomData.playerList[0].id);
                OutGameUI.instance.PopupMatchMakingPanel();

                // 파싱된 데이터 출력
                foreach (MatchInfo player in roomData.playerList)
                {
                    OutGameUI.instance.DrawMatchPlayer(player.name);
                }
                OutGameUI.instance.ReturnMatchMakingUI();
            });
        });

        socket.On("loadGameScene", (res) =>
        {
            Debug.Log(res);
            UnityThread.executeInLateUpdate(() =>
            {
                OutGameUI.instance.LoadInGame();
            });
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

    public RoomData ParseData(string jsonData)
    {
        RoomData roomData = JsonConvert.DeserializeObject<RoomData>(jsonData);
        return roomData;
    }

    public void LoginSucc(string email)
    {
        LoginInfo sendPacket = new LoginInfo();
        sendPacket.email = email;
        Debug.Log("보낸다 : "+sendPacket);
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
        Debug.Log("matchmaking id 보낸다 : "+sendPacket.id);
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
        Debug.Log("setName 보낸다 : " + sendPacket);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("setName", jsonData);
    }

#endregion
}