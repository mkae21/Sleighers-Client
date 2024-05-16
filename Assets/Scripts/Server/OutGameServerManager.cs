using Newtonsoft.Json;
using UnityEngine;
/* OutGameServerManager.cs
 * - 아웃게임 서버와의 통신을 관리
 */
public partial class ServerManager : MonoBehaviour
{
#region PrivateVariables
    private SocketIOUnity socket;
    private string outGameServerIP = string.Empty;
    private int outGameServerPort = 0;
#endregion

#region PublicVariables
    public static ServerManager instance = null;
    public RoomData roomData;
    public bool isConnectOutGame = false;
#endregion

#region PrivateMethod
    private void ConnectOutGame()
    {
        socket = new SocketIOUnity($"http://{outGameServerIP}:{outGameServerPort}");

        socket.OnConnected += (sender, e) =>
        {
            isConnectOutGame = true;
            Debug.LogFormat("[OutGameServerManager] 서버 접속 성공 {0}:{1}", inGameServerIP, inGameServerPort);
        };

        // 연결 해제 이벤트 핸들러
        socket.OnDisconnected += (sender, e) =>
        {
            isConnectOutGame = false;
            Debug.LogFormat("[OutGameServerManager] 서버 접속 해제 {0}:{1}", inGameServerIP, inGameServerPort);
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
            UnityThread.executeInLateUpdate (() =>
            {
                OutGameUI.instance.SuccLoginPanel();
            });
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
            Debug.Log("inquiryPlayer: 내 플레이어 정보 받음: " + res);
            string jsonString = res.GetValue<string>();
            PlayerInfo userInfo = JsonUtility.FromJson<PlayerInfo>(jsonString);
            UserData.instance.nickName = userInfo.nickname;
            UserData.instance.email = userInfo.email;
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

                Debug.Log(roomData.playerList[0].email);
                OutGameUI.instance.PopupMatchMakingPanel();

                // 파싱된 데이터 출력
                foreach (PlayerInfo player in roomData.playerList)
                {
                    OutGameUI.instance.DrawMatchPlayer(player.nickname);
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

    private RoomData ParseData(string jsonData)
    {
        RoomData roomData = JsonConvert.DeserializeObject<RoomData>(jsonData);
        return roomData;
    }
    private void MatchMaking()
    {
        if (!isConnectOutGame)
            return;
        PlayerInfo sendPacket = new PlayerInfo
        {
            email = UserData.instance.email
        };
        Debug.Log("matchmaking id 보낸다 : "+sendPacket.email);
        string jsonData = JsonUtility.ToJson(sendPacket);
        socket.Emit("matching", jsonData);
        OutGameUI.instance.MatchMakingUI();
    }
    private void SocketEmit(API.Type _type, PlayerInfo _playerInfo)
    {
        string jsonData = JsonUtility.ToJson(_playerInfo);
        socket.Emit(_type.ToString(), jsonData);
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
    public void OnSendOutGame(API.Type _type, PlayerInfo _playerInfo)
    {
        if (!isConnectOutGame)
            return;
        Debug.LogFormat("[ServerManager] OnSendOutGame: {0} / {1}", _type, _playerInfo.ToString());
        switch (_type)
        {
            case API.Type.login:
                SocketEmit(_type, _playerInfo);
                break;

            case API.Type.loginSucc:
                SocketEmit(_type, _playerInfo);
                break;

            case API.Type.signup:
                SocketEmit(_type, _playerInfo);
                break;

            case API.Type.setName:
                SocketEmit(_type, _playerInfo);
                break;

            case API.Type.matching:

                break;

            default:
                Debug.LogWarning("[ServerManager] OnSendOutGame: 잘못된 API 타입입니다.");
                break;
        }
    }

#endregion
}