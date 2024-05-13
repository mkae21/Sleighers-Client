using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;
using Protocol;
using System.Threading.Tasks;
using System;
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
    private GameObject playerPrefab;
    private SessionInfo sessionInfo;
    private Dictionary<int, Player> players;
    private Transform[] startingPoints;
    private Queue<byte[]> messageQueue = new Queue<byte[]>();
    private int tick = 0;
#endregion

#region PublicVariables
    static public WorldManager instance;
    public int myPlayerId { get; private set; } = -1;
    public bool isGameStart { get; private set; } = false;
    public bool isRaceFinish { get; private set; } = false;
    public GameObject playerPool;
    public Transform startingPointHolder;
    public MiniMapController miniMapController;
    // 레이스가 종료되면 호출되는 액션
    public UnityAction OnRaceFinished { get; set; }

#endregion

#region PrivateMethod
    // 애플리케이션 종료 시
    private void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        ServerManager.Instance().IsConnect = false;
        if (ServerManager.Instance().Stream != null)
            ServerManager.Instance().CloseStream();
        if (ServerManager.Instance().Client != null)
            ServerManager.Instance().CloseTcpClient();
    }
    private void Awake()
    {
        instance = this;
        rankManager = GetComponent<RankManager>();
        players = new Dictionary<int, Player>();
        rankManager.OnLapComplete += OnLapComplete;
        OnRaceFinished += FinishRace;
    }
    private void Start()
    {
        InitializeGame();
    }
    private void FixedUpdate()
    {
        if (tick % 2 == 0)
            OnSend(Protocol.Type.Sync);
        tick++;
        OnReceive();
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
    }
    // 플레이어가 한 바퀴를 완주했을 때 호출되는 콜백
    private void OnLapComplete(Player _player, RankInfo _lapInfo)
    {
        // 플레이어가 레이스를 완료했나 확인
        if (_player.isMe && _lapInfo.lap == rankManager.Laps)
        {
            // 아직 완료하지 못했다면 레이스를 완료
            if (!isRaceFinish)
            {
                OnRaceFinished?.Invoke(); 
            }
        }
    }

    // 레이스를 완료하고 아웃트로 타임라인을 재생
    private void FinishRace()
    {
        isRaceFinish = true;
        OnSend(Protocol.Type.PlayerGoal);
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
            if (msg.from == myPlayerId)
            {
                Debug.LogWarning("[OnReceive] 내 플레이어의 메세지입니다.");
                return;
            }
            if (msg.type != Protocol.Type.Sync)
            {
                Debug.LogFormat("[OnReceive] 메세지 타입 : {0}", msg.type);
                LogManager.instance.Log("[OnReceive] 메세지 타입 :" + msg.type.ToString());
            }
            switch (msg.type)
            {
                case Protocol.Type.LoadGameScene:
                    LoadGameSceneMessage loadMessage = DataParser.ReadJsonData<LoadGameSceneMessage>(data);
                    ReceiveLoadGameSceneEvent(loadMessage);
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
                    GameResultMessage gameResultMessage = DataParser.ReadJsonData<GameResultMessage>(data);
                    ReceiveGameEndEvent(gameResultMessage);
                    break;
                
                case Protocol.Type.Sync:
                    SyncMessage syncMessage = DataParser.ReadJsonData<SyncMessage>(data);
                    ReceiveSyncEvent(syncMessage);
                    break;

                case Protocol.Type.PlayerReconnect:
                    ReceivePlayerReconnectEvent(msg);
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
            int id = msg.from;
            Vector3 position = msg.position;
            Vector3 velocity = msg.velocity;
            float rotation = msg.rotation;
            long timeStamp = msg.timeStamp;
            players[id].SetSyncData(position, velocity, rotation, timeStamp);
        });
    }

    // 다른 플레이어 접속 이벤트 처리
    private void ReceivePlayerReconnectEvent(Message msg)
    {
        int newId = msg.from;
        Transform sp = startingPoints[sessionInfo.totalPlayerCount];
        GameObject newInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerPool.transform);
        newInstance.GetComponent<Player>().Initialize(false, newId, "Player" + newId, sp.position, sp.rotation.eulerAngles.y);
        players.Add(newId, newInstance.GetComponent<Player>());
        sessionInfo.totalPlayerCount++;
    }
    // 게임 씬 로드 이벤트 처리
    private void ReceiveLoadGameSceneEvent(LoadGameSceneMessage msg)
    {
        myPlayerId = msg.from;
        int totalPlayerCount = msg.count;
        sessionInfo.totalPlayerCount = totalPlayerCount + 1;
        List<int> userList = msg.list;
        Transform sp = startingPoints[totalPlayerCount].transform;

        GameObject myPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerPool.transform);
        myPlayer.GetComponent<Player>().Initialize(true, myPlayerId, "Player" + myPlayerId, sp.position, sp.rotation.eulerAngles.y);
        Transform miniMapTarget = myPlayer.transform.Find("Sled");
        miniMapController.SetTarget(miniMapTarget);
        players.Add(myPlayerId, myPlayer.GetComponent<Player>());
        Debug.LogFormat("[WorldManager] 내 플레이어 생성 완료 : {0}", myPlayerId);

        for (int i = 0; i < totalPlayerCount; i++)
        {
            int otherPlayerId = userList[i];
            Transform _sp = startingPoints[i].transform;
            GameObject otherPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity, playerPool.transform);
            otherPlayer.GetComponent<Player>().Initialize(false, otherPlayerId, "Player" + otherPlayerId, _sp.position, _sp.rotation.eulerAngles.y);
            players.Add(otherPlayerId, otherPlayer.GetComponent<Player>());
        }
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
    private void ReceiveGameEndEvent(GameResultMessage msg)
    {
        GameManager.Instance().ChangeState(GameManager.GameState.End, msg);
    }
    // 다른 플레이어 접속 끊김 이벤트 처리
    private void ReceivePlayerDisconnectEvent(Message msg)
    {
        int userId = msg.from;
        Destroy(players[userId].gameObject);
        RankManager.instance.DeleteRankInfo(players[userId]);
        players.Remove(userId);
        sessionInfo.totalPlayerCount--;
    }
#endregion

#region Send 프로토콜 처리
    // 동기화 이벤트를 서버에 알림
    private void SendSyncEvent()
    {
        if (players == null || !isGameStart)
            return;
        SyncMessage msg = GetMyPlayer().GetSyncData();
        ServerManager.Instance().SendDataToInGame(msg);
    }

    // 게임 시작 이벤트를 서버에 알림
    private async void SendGameStartEvent()
    {
        await Task.Run(() =>
        {
            if (isGameStart)
                return;
            Message msg = new Message(Protocol.Type.GameStart, myPlayerId);
            ServerManager.Instance().SendDataToInGame(msg);
        });
    }
    // 내 플레이어가 골인했음을 서버에 알림
    private async void SendPlayerGoalEvent()
    {
        await Task.Run(() =>
        {
            Message msg = new Message(Protocol.Type.PlayerGoal, myPlayerId);
            ServerManager.Instance().SendDataToInGame(msg);
        });
    }
    // 임시 서버 리셋
    private async void SendResetServerEvent()
    {
        await Task.Run(() =>
        {
            Message msg = new Message(Protocol.Type.ResetServer, myPlayerId);
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
            Debug.LogErrorFormat("[Polling] 데이터 수신 중 에러 발생 : {0}", ex.Message);
        }
    }
    // 서버로 보내는 데이터 처리 핸들러
    public void OnSend(Protocol.Type _type)
    {
        if (_type != Protocol.Type.Sync)
        {
            Debug.LogFormat("[OnSend] 메세지 타입 : {0}", _type);
            LogManager.instance.Log("[OnSend] 메세지 타입 : " + _type.ToString());
        }
        switch (_type)
        {
            case Protocol.Type.Sync:
                SendSyncEvent();
                break;

            case Protocol.Type.GameStart:
                SendGameStartEvent();
                break;

            case Protocol.Type.PlayerGoal:
                SendPlayerGoalEvent();
                break;

            case Protocol.Type.ResetServer:
                SendResetServerEvent();
                break;

            default:
                Debug.LogWarning("[OnSend] 알 수 없는 프로토콜 : "+ _type);
                break;
        }
    }

    public Player GetMyPlayer()
    {
        return players[myPlayerId];
    }
    public GameObject GetMySled()
    {
        return players[myPlayerId].gameObject.transform.Find("Sled").gameObject;
    }
    public Vector3 GetMyPlayerPosition()
    {
        return players[myPlayerId].GetPosition();
    }
    public Vector3 GetMyPlayerVelocity()
    {
        return players[myPlayerId].GetVelocity();
    }
    public float GetMyPlayerRotationY()
    {
        return players[myPlayerId].GetRotationY();
    }
    public Player GetPlayerFromId(int _id)
    {
        return players[_id];
    }
#endregion
}