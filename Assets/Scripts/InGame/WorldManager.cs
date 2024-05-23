using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Protocol;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Text;
/* WorldManager.cs
 * - 인게임 내의 모든 것을 관리
 * - 인게임 내에서 프로토콜 수신 및 처리
 * - 인게임 내에서 플레이어 생성 및 삭제 및 접근
 */
public class WorldManager : MonoBehaviour
{
    class SessionInfo
    {
        public int totalPlayerCount;
    }
#region PrivateVariables
    private RankManager rankManager;
    private Dictionary<string, Player> players;
    private GameObject playerPrefab;
    private SessionInfo sessionInfo;
    private Transform[] startingPoints;
    private Queue<byte[]> messageQueue = new Queue<byte[]>();
    private int tick = 0;
    [SerializeField]
    private Material[] playerMaterials;
#endregion

#region PublicVariables
    static public WorldManager instance;
    public string myPlayerNickname { get; private set; } = string.Empty;
    public bool isGameStart { get; private set; } = false;
    public bool isRaceFinish { get; private set; } = false;
    public GameObject playerPool;
    public Transform startingPointHolder;
    public MiniMapController miniMapController;
    // public List<GameObject> allPlayerList = new List<GameObject>();
#endregion

#region PrivateMethod
    // 애플리케이션 종료 시
    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        if (ServerManager.Instance().Stream != null && ServerManager.Instance().Client != null)
            ServerManager.Instance().DisconnectInGame();
    }
    private void Awake()
    {
        instance = this;
        rankManager = GetComponent<RankManager>();
        players = new Dictionary<string, Player>();
        rankManager.OnFinish += OnFinish;
    }
    private void Start()
    {
        InitializeGame();
        OfflineTest();
    }
    private void FixedUpdate()
    {
        if (tick % 4 == 0)
            OnSendInGame(Protocol.Type.Sync);
        tick++;
        OnReceive();
    }
    // 서버에 접속이 되지 않으면 오프라인 테스트 진행
    private void OfflineTest()
    {
        if (ServerManager.Instance().isConnectInGame)
            return;
        Debug.LogWarning("[WorldManager] 서버에 접속되지 않았습니다. 오프라인 테스트 진행");
        GameObject testPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerPool.transform);
        players.Add(myPlayerNickname, testPlayer.GetComponent<Player>());
        Transform sp = startingPoints[0];
        testPlayer.GetComponent<Player>().Initialize(true, "TestPlayer", sp.position, sp.rotation.eulerAngles.y);
        Transform miniMapTarget = testPlayer.transform.Find("Sled");
        miniMapController.SetTarget(miniMapTarget);
        GameManager.Instance().ChangeState(GameManager.GameState.InGame);
        Debug.Log("[WorldManager] 테스트 플레이어 생성 완료");
    }
    // 인게임 초기화
    private void InitializeGame()
    {
        if (!playerPool)
        {
            Debug.Log("[World Manager] Player Pool이 존재하지 않습니다.");
            return;
        }
        Debug.Log("[World Manager] 게임 초기화 진행");

        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;
        sessionInfo = new SessionInfo();
        startingPoints = new Transform[startingPointHolder.childCount];
        for (int i = 0; i < startingPointHolder.childCount; i++)
            startingPoints[i] = startingPointHolder.GetChild(i);
        OnSendInGame(Protocol.Type.GameSetUp);
        Timeline.instance.StartTimeline();
    }
    // 플레이어가 한 바퀴를 완주했을 때 호출되는 콜백
    private void OnFinish(Player _player, RankInfo _rankInfo)
    {
        if (!_player.isMe)
            return;

        // 아직 완료하지 못했다면 레이스를 완료하고 아웃트로 타임라인을 재생
        if (!isRaceFinish)
        {
            isRaceFinish = true;
            OnSendInGame(Protocol.Type.PlayerGoal);
        }
    }
#region Receive 프로토콜 처리
    // 서버로부터 받은 데이터 처리 핸들러
    private void OnReceive()
    {
        while (messageQueue.Count > 0)
        {
            byte[] data;
            lock (messageQueue)
                data = messageQueue.Dequeue();

            Message msg = DataParser.ReadJsonData<Message>(data);
            if (msg == null)
            {
                Debug.LogWarning("[OnReceive] 메세지가 비어있습니다.");
                return;
            }
            if (msg.from == myPlayerNickname)
            {
                Debug.LogWarning("[OnReceive] 내 플레이어의 메세지입니다.");
                return;
            }
            if (msg.type != Protocol.Type.Sync)
            {
                Debug.LogFormat("[OnReceive] 메세지 타입 : {0}", msg.type);
            }
            switch (msg.type)
            {
                case Protocol.Type.GameSetUp:
                    ReceiveGameSetUp(ServerManager.instance.roomData.playerList);
                    break;

                case Protocol.Type.GameStartCountDown:
                    GameCountDownMessage startCountMessage = DataParser.ReadJsonData<GameCountDownMessage>(data);
                    ReceiveGameStartCountDownEvent(startCountMessage);
                    break;

                case Protocol.Type.GameEndCountDown:
                    GameCountDownMessage endCountMessage = DataParser.ReadJsonData<GameCountDownMessage>(data);
                    ReceiveGameEndCountDownEvent(endCountMessage);
                    break;

                case Protocol.Type.GameStart:
                    ReceiveGameStartEvent();
                    break;

                case Protocol.Type.GameEnd:
                    GameEndStruct gameResultStruct = JsonConvert.DeserializeObject<GameEndStruct>(Encoding.Default.GetString(data));
                    GameEndMessage gameResultMessage = new GameEndMessage(gameResultStruct.roomID, gameResultStruct.from, gameResultStruct.resultList, gameResultStruct.endTime);
                    ReceiveGameEndEvent(gameResultMessage);
                    break;
                
                case Protocol.Type.Sync:
                    SyncMessage syncMessage = DataParser.ReadJsonData<SyncMessage>(data);
                    ReceiveSyncEvent(syncMessage);
                    break;

                case Protocol.Type.PlayerDisconnect:
                    ReceivePlayerDisconnectEvent(msg);
                    break;

                default:
                    Debug.LogWarning("[OnReceive] 알 수 없는 메시지 타입: " + msg.type);
                    break;
            }
        }
    }

    // 동기화 이벤트 처리
    private async void ReceiveSyncEvent(SyncMessage msg)
    {
        await Task.Run(() =>
        {
            if (players == null || !isGameStart)
                return;
            string id = msg.from;
            Vector3 position = msg.position;
            Vector3 velocity = msg.velocity;
            Quaternion rotation = msg.rotation;
            long timeStamp = msg.timeStamp;

            Player player = GetPlayerFromNickName(id);
            if (player == null)
                return;
            player.SetSyncData(position, velocity, rotation, timeStamp);
        });
    }

    // 게임 씬 로드 이벤트 처리
    private void ReceiveGameSetUp(List<PlayerInfo> playerList)
    {
        myPlayerNickname = ServerManager.instance.myNickname;
        int myidx = 0;
        int totalPlayerCount = playerList.Count;
        sessionInfo.totalPlayerCount = playerList.Count;

        for (int i = 0; i < totalPlayerCount; i++)
        {
            string playerNickname = playerList[i].nickname;
            if (playerNickname == myPlayerNickname)
            {
                myidx = i;
                break;
            }
        }
        Transform sp = startingPoints[myidx].transform;
        GameObject myPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerPool.transform);
        GameObject myModel = myPlayer.transform.Find("Sled").Find("Character").Find("Character_Male").gameObject;
        myModel.GetComponent<Renderer>().material = playerMaterials[myidx];
        players.Add(myPlayerNickname, myPlayer.GetComponent<Player>());
        myPlayer.GetComponent<Player>().Initialize(true, ServerManager.instance.myNickname, sp.position, sp.rotation.eulerAngles.y);
        myPlayer.GetComponent<Player>().playerIndex = myidx;
        Transform miniMapTarget = myPlayer.transform.Find("Sled");
        miniMapController.SetTarget(miniMapTarget);
        Debug.LogFormat("[WorldManager] 내 플레이어 생성 완료 : {0}", myPlayerNickname);

        for (int i = 0; i < totalPlayerCount; i++)
        {
            string playerNickname = playerList[i].nickname;
            if (playerNickname == myPlayerNickname)
                continue;
            Transform _sp = startingPoints[i].transform;
            GameObject Player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerPool.transform);
            GameObject characterModel = Player.transform.Find("Sled").Find("Character").Find("Character_Male").gameObject;
            characterModel.GetComponent<Renderer>().material = playerMaterials[i];
            players.Add(playerNickname, Player.GetComponent<Player>());
            Player.GetComponent<Player>().Initialize(false, playerNickname, _sp.position, _sp.rotation.eulerAngles.y);
            Player.GetComponent<Player>().playerIndex = i;
        }
        
        ObstacleHolder.instance.SetObstacles(players);
    }

    // 게임 시작 카운트 다운 이벤트 처리
    private void ReceiveGameStartCountDownEvent(GameCountDownMessage msg)
    {
        int count = msg.count;
        InGameUI.instance.SetCountDown(count);
    }
    // 게임 시작 이벤트 처리
    private void ReceiveGameStartEvent()
    {   
        isGameStart = true;
        GameManager.Instance().ChangeState(GameManager.GameState.InGame);
    }
    // 게임 종료 카운트 다운 이벤트 처리
    private void ReceiveGameEndCountDownEvent(GameCountDownMessage msg)
    {
        int count = msg.count;
        InGameUI.instance.SetGameEndCountDown(count);        
    }
    // 게임 종료 이벤트 처리
    private void ReceiveGameEndEvent(GameEndMessage msg)
    {
        GameManager.Instance().ChangeState(GameManager.GameState.End, msg);
    }
    // 다른 플레이어 접속 끊김 이벤트 처리
    private void ReceivePlayerDisconnectEvent(Message msg)
    {
        string userId = msg.from;
        Destroy(players[userId].gameObject);
        RankManager.instance.DeleteRankInfo(players[userId]);
        players.Remove(userId);
        sessionInfo.totalPlayerCount--;
    }
#endregion

#region Send 프로토콜 처리
    // 게임 셋업 이벤트를 서버에 알림
    private void SendGameSetUpEvent()
    {
        Message msg = new Message(Protocol.Type.GameSetUp, ServerManager.instance.roomData.roomID, ServerManager.instance.myNickname);
        ServerManager.Instance().SendDataToInGame(msg);
    }
    // 플레이어 준비 완료 이벤트를 서버에 알림
    private void SendPlayerReadyEvent()
    {
        Message msg = new Message(Protocol.Type.PlayerReady, ServerManager.instance.roomData.roomID, ServerManager.instance.myNickname);
        ServerManager.Instance().SendDataToInGame(msg);
    }
    // 동기화 이벤트를 서버에 알림
    private void SendSyncEvent()
    {
        if (players == null || !isGameStart)
            return;
        SyncMessage msg = GetMyPlayer().GetSyncData();
        ServerManager.Instance().SendDataToInGame(msg);
    }

    // 내 플레이어가 골인했음을 서버에 알림
    private async void SendPlayerGoalEvent()
    {
        await Task.Run(() =>
        {
            Message msg = new Message(Protocol.Type.PlayerGoal, ServerManager.instance.roomData.roomID, myPlayerNickname);
            ServerManager.Instance().SendDataToInGame(msg);
        });
    }

    private int ReadBytes(byte[] buffer, int offset, int count, int timeoutCounts)
    {
        NetworkStream stream = ServerManager.Instance().Stream;
        int bytesRead = 0;
        int cnt = 0;
        while (bytesRead < count)
        {
            cnt++;
            int readSize = stream.Read(buffer, offset + bytesRead, count - bytesRead);
            if (readSize == 0)
            {
                if (cnt > timeoutCounts)
                    throw new TimeoutException("읽기 타임아웃");
            }
            else
                bytesRead += readSize;
        }
        return bytesRead;
    }
#endregion


#endregion

#region PublicMethod
    // 서버로부터 데이터를 받아오는 함수
    public void Polling()
    {
        const int timeoutCount = 100; // 100번 루프 제한
        byte[] buffer = new byte[4096];
        try
        {
            // 메세지 크기 읽기
            int bytesRead = ReadBytes(buffer, 0, 4, timeoutCount);
            if (bytesRead != 4)
            {
                Debug.LogWarningFormat("[Polling] 메세지 크기 읽기 실패 : {0}", bytesRead);
                return;
            }

            // 메세지 데이터 읽기
            int jsonSize = BitConverter.ToInt32(buffer, 0);
            bytesRead = ReadBytes(buffer, 0, jsonSize, timeoutCount);
            if (bytesRead != jsonSize)
            {
                Debug.LogWarningFormat("[Polling] 메세지 데이터 읽기 실패 : {0}", bytesRead);
                return;
            }

            // 메모리 할당 최적화를 위해 데이터 복사
            byte[] messageData = new byte[jsonSize];
            Array.Copy(buffer, 0, messageData, 0, jsonSize);
            lock (messageQueue)
            {
                messageQueue.Enqueue(messageData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarningFormat("[Polling] 데이터 수신 중 에러 발생 : {0}", ex.Message);
        }
    }
    // 인게임 서버로 보내는 데이터 처리 핸들러
    public void OnSendInGame(Protocol.Type _type)
    {
        if (!ServerManager.Instance().isConnectInGame)
            return;
        if (_type != Protocol.Type.Sync)
        {
            Debug.LogFormat("[OnSend] 메세지 타입 : {0}", _type);
        }
        switch (_type)
        {
            case Protocol.Type.Sync:
                SendSyncEvent();
                break;

            case Protocol.Type.GameSetUp:
                SendGameSetUpEvent();
                break;

            case Protocol.Type.PlayerReady:
                SendPlayerReadyEvent();
                break;

            case Protocol.Type.PlayerGoal:
                SendPlayerGoalEvent();
                break;

            default:
                Debug.LogWarning("[OnSend] 알 수 없는 프로토콜 : "+ _type);
                break;
        }
    }

    public Player GetMyPlayer()
    {
        return GetPlayerFromNickName(myPlayerNickname);
    }
    public GameObject GetMySled()
    {
        return players[myPlayerNickname].gameObject.transform.Find("Sled").gameObject;
    }
    public Player GetPlayerFromNickName(string _nickname)
    {
        if (players.ContainsKey(_nickname))
            return players[_nickname];
        else
            return null;
    }
#endregion
}