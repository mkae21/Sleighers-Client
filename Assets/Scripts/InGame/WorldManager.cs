using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Protocol;
using Reader;
using System;
/* WorldManager.cs
 * - 인게임 내의 모든 것을 관리
 * - 인게임 내에서 프로토콜 수신 및 처리
 * - 인게임 내에서 플레이어 생성 및 삭제
 */
public class WorldManager : MonoBehaviour
{
    class SessionInfo
    {
        public int totalPlayerCount;
    }
#region PrivateVariables
    private GameObject playerPrefab;
    private SessionInfo sessionInfo;
    private Dictionary<int, Player> players;
    [SerializeField] private int myPlayerId;
    public int MyPlayerId
    {
        get { return myPlayerId; }
        set { myPlayerId = value; }
    }
    private bool isGameStart = false;
#endregion

#region PublicVariables
    static public WorldManager instance;
    public GameObject playerPool;
    public Transform[] startingPoints;
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
    }
    private void Start()
    {
        players = new Dictionary<int, Player>();
        sessionInfo = new SessionInfo();
        InitializeGame();
    }

    // 인게임 초기화
    private bool InitializeGame()
    {

        if (!playerPool)
        {
            Debug.Log("[World Manager] Player Pool이 존재하지 않습니다.");
            return false;
        }
        Debug.Log("[World Manager] 게임 초기화 진행");

        playerPrefab = Resources.Load("Prefabs/Player") as GameObject;

        return true;
    }


    // 서버로부터 받는 데이터 처리 핸들러
    public void OnReceive()
    {
        NetworkStream stream = ServerManager.Instance().Stream;
        byte[] size = new byte[4];
        stream.Read(size, 0, size.Length);

        ByteReader br = new ByteReader(size);
        int jsonSize = br.ReadInt();

        byte[] data = new byte[jsonSize];
        int receiveSize = stream.Read(data, 0, data.Length);
        
        if (receiveSize == 0)
        {
            Debug.Log("[OnReceive] 빈 데이터가 브로드캐스킹 되었습니다.");
            return;
        }
        Message msg = DataParser.ReadJsonData<Message>(data);
        if (msg == null)
            return;
        if (ServerManager.Instance().IsHost() == true && msg.id == MyPlayerId)
            return;
        if (players == null)
            return;
        Debug.LogFormat("[OnReceive] 받은 데이터 타입 : {0}", msg.type);
        switch(msg.type)
        {
            case Protocol.Type.Key:
                KeyMessage keyMessage = DataParser.ReadJsonData<KeyMessage>(data);
                ReceiveKeyEvent(keyMessage);
                break;
            case Protocol.Type.PlayerBreak:
                PlayerBreakMessage breakMessage = DataParser.ReadJsonData<PlayerBreakMessage>(data);
                ReceivePlayerBreakEvent(breakMessage);
                break;
            case Protocol.Type.PlayerMove:
                PlayerMoveMessage moveMessage = DataParser.ReadJsonData<PlayerMoveMessage>(data);
                ReceivePlayerMoveEvent(moveMessage);
                break;

            // case Protocol.Type.PlayerReconnect:
            //     break;

            case Protocol.Type.PlayerDisconnect:
                ReceivePlayerDisconnectEvent(msg);
                break;

            case Protocol.Type.OtherPlayerConnect:
                ReceiveOtherPlayerConnectEvent(msg);
                break;

            // case Protocol.Type.OtherPlayerReconnect:
            //     break;

            case Protocol.Type.GameSync:
                GameSyncMessage syncMessage = DataParser.ReadJsonData<GameSyncMessage>(data);
                ReceiveGameSyncEvent(syncMessage);
                break;

            case Protocol.Type.LoadGameScene:
                LoadGameSceneMessage loadMessage = DataParser.ReadJsonData<LoadGameSceneMessage>(data);
                ReceiveLoadGameSceneEvent(loadMessage);
                break;

            case Protocol.Type.GameStartCountDown:
                GameStartCountDownMessage startCountMessage = DataParser.ReadJsonData<GameStartCountDownMessage>(data);
                ReceiveGameStartCountDownEvent(startCountMessage);
                break;

            case Protocol.Type.GameStart:
                ReceiveGameStartEvent();
                break;

            case Protocol.Type.PlayerGoal:
                break;

            case Protocol.Type.GameEndCountDown:
                break;

            // case Protocol.Type.GameEnd:
            //     ReceiveGameEndEvent(br);
            //     break;

            default:
                Debug.LogWarning("[OnReceive] 알 수 없는 프로토콜");
                break;
        }
    }
    public void OnRecieveForLocal(KeyMessage keyMessage)
    {
        ReceiveKeyEvent(keyMessage);
    }
#region Receive 프로토콜 처리
    // 키 입력 이벤트 처리
    private void ReceiveKeyEvent(KeyMessage keyMessage)
    {
        if (ServerManager.Instance().IsHost() == false)
            return;
        bool isMove = false;
        bool isBreak = false;
        int keyData = keyMessage.keyData;
        int id = keyMessage.id;

        Vector3 playerPos = players[id].GetPosition();
        Vector3 playerDir = Vector3.zero;
        if ((keyData & KeyEventCode.MOVE) == KeyEventCode.MOVE)
        {
            playerDir = keyMessage.position;
            playerDir = Vector3.Normalize(playerDir);
            isMove = true;
        }
        else if ((keyData & KeyEventCode.BREAK) == KeyEventCode.BREAK)
        {
            isBreak = true;
        }

        if (isMove)
        {
            players[id].SetMoveVector(playerDir);
            PlayerMoveMessage msg = new PlayerMoveMessage(id, playerPos, playerDir);
            ServerManager.Instance().SendDataToInGame<PlayerMoveMessage>(msg);
        }
        if (isBreak)
        {
            players[id].IsBraking = true;
            PlayerBreakMessage msg = new PlayerBreakMessage(id);
            ServerManager.Instance().SendDataToInGame<PlayerBreakMessage>(msg);
        }
    }
    // 플레이어 이동 이벤트 처리
    private void ReceivePlayerMoveEvent(PlayerMoveMessage msg)
    {
        if (ServerManager.Instance().IsHost() == true)
            return;
        Debug.LogFormat("[OnReceive] ReceivePlayerMoveEvent : {0}", msg.id);
        Vector3 moveVector = msg.direction;
        // moveVector가 같으면 방향 & 이동량 같으므로 적용 굳이 안함
        if (!moveVector.Equals(players[msg.id].moveVector))
        {
            // players[msg.id].SetPosition(msg.position);
            players[msg.id].SetMoveVector(moveVector);
        }
    }
    private void ReceivePlayerBreakEvent(PlayerBreakMessage msg)
    {
        if (ServerManager.Instance().IsHost() == true)
            return;
        Debug.LogFormat("[OnReceive] ReceivePlayerBreakEvent : {0}", msg.id);
        players[msg.id].IsBraking = true;
    }
    // 다른 플레이어 접속 이벤트 처리
    private void ReceiveOtherPlayerConnectEvent(Message msg)
    {
        int newId = msg.id;
        Transform sp = startingPoints[sessionInfo.totalPlayerCount];
        GameObject newInstance = Instantiate(playerPrefab, sp.position, sp.rotation, playerPool.transform);
        newInstance.GetComponent<Player>().Initialize(false, newId, "Player" + newId);
        players.Add(newId, newInstance.GetComponent<Player>());
        sessionInfo.totalPlayerCount++;
    }
    // 게임 씬 로드 이벤트 처리
    private void ReceiveLoadGameSceneEvent(LoadGameSceneMessage msg)
    {
        myPlayerId = msg.id;
        int totalPlayerCount = msg.count;
        sessionInfo.totalPlayerCount = totalPlayerCount + 1;
        List<int> userList = msg.list;
        if (msg.ishost)
        {
            ServerManager.Instance().SetHostSession(myPlayerId);
        }

        Transform sp = startingPoints[totalPlayerCount].transform;
        GameObject myPlayer = Instantiate(playerPrefab, sp.position, sp.rotation, playerPool.transform);
        myPlayer.GetComponent<Player>().Initialize(true, myPlayerId, "Player" + myPlayerId);
        players.Add(myPlayerId, myPlayer.GetComponent<Player>());
        Debug.LogFormat("[WorldManager] 내 플레이어 생성 완료 : {0}", myPlayerId);
        for (int i = 0; i < totalPlayerCount; i++)
        {
            int otherPlayerId = userList[i];
            Transform _sp = startingPoints[i].transform;
            GameObject otherPlayer = Instantiate(playerPrefab, _sp.position, _sp.rotation, playerPool.transform);
            otherPlayer.GetComponent<Player>().Initialize(false, otherPlayerId, "Player" + otherPlayerId);
            players.Add(otherPlayerId, otherPlayer.GetComponent<Player>());
        }
    }
    // 게임 시작 카운트 다운 이벤트 처리
    private void ReceiveGameStartCountDownEvent(GameStartCountDownMessage msg)
    {
        int count = msg.count;
        Debug.LogFormat("[OnReceive] GameStartCountDownEvent : {0}", count);
    }
    // 게임 시작 이벤트 처리
    private void ReceiveGameStartEvent()
    {
        Debug.LogFormat("[OnReceive] ReceiveGameStartEvent");
        GameManager.Instance().ChangeState(GameManager.GameState.InGame);
    }
    // 다른 플레이어 접속 끊김 이벤트 처리
    private void ReceivePlayerDisconnectEvent(Message msg)
    {
        int userId = msg.id;
        Destroy(players[userId].gameObject);
        players.Remove(userId);
    }
    // 플레이어 위치 및 회전 동기화 이벤트 처리
    private void ReceiveGameSyncEvent(GameSyncMessage msg)
    {
        int index = 0;
        if (players == null)
            return;

        foreach (var player in players)
        {
            player.Value.SetPosition(msg.positions[index]);
            index++;
        }
        // ServerManager.Instance().SetHostSession(syncMessage.host);
    }
    // 게임 종료 이벤트 처리
    private void ReceiveGameEndEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Debug.LogFormat("플레이어 {0}가 승리했습니다.", userId);
    }
#endregion

#region Send 프로토콜 처리
    // 호스트의 게임 정보를 서버에 보내서 모든 플레이어에게 동기화
    private void SendGameSyncEvent()
    {
        int numOfClient = players.Count;

        Vector3[] positions = new Vector3[numOfClient];
        bool[] online = new bool[numOfClient];
        int index = 0;
        foreach (var player in players)
        {
            positions[index] = player.Value.GetPosition();
            index++;
        }
        GameSyncMessage msg = new GameSyncMessage(myPlayerId, numOfClient, positions, online);
        ServerManager.Instance().SendDataToInGame(msg);
    }
    // 게임 시작 이벤트를 서버에 알림
    private void SendGameStartEvent()
    {
        if (isGameStart)
            return;
        isGameStart = true;
        Debug.LogFormat("[OnSend] SendGameStartEvent");
        Message msg = new Message(Protocol.Type.GameStart, myPlayerId);
        ServerManager.Instance().SendDataToInGame(msg);
    }
    // 내 플레이어가 골인했음을 서버에 알림
    private void SendPlayerGoalEvent()
    {
        Debug.LogFormat("[OnSend] SendPlayerGoalEvent");
        Message msg = new Message(Protocol.Type.PlayerGoal, myPlayerId);
        ServerManager.Instance().SendDataToInGame(msg);
    }
    // 임시 서버 리셋
    private void SendResetServerEvent()
    {
        Debug.LogFormat("[OnSend] SendResetServerEvent");
        Message msg = new Message(Protocol.Type.ResetServer, myPlayerId);
        ServerManager.Instance().SendDataToInGame(msg);
    }
#endregion


#endregion

#region PublicMethod
    // 서버로 보내는 데이터 처리 핸들러
    public void OnSend(Protocol.Type _type)
    {
        switch (_type)
        {
            case Protocol.Type.PlayerMove:
                break;

            case Protocol.Type.PlayerReconnect:
                break;
                
            case Protocol.Type.GameSync:
                SendGameSyncEvent();
                break;

            case Protocol.Type.LoadGameScene:
                break;

            case Protocol.Type.GameStartCountDown:
                break;

            case Protocol.Type.GameStart:
                SendGameStartEvent();
                break;

            case Protocol.Type.PlayerGoal:
                SendPlayerGoalEvent();
                break;

            case Protocol.Type.GameEndCountDown:
                break;

            case Protocol.Type.GameEnd:
                break;

            case Protocol.Type.ResetServer:
                SendResetServerEvent();
                break;

            default:
                Debug.LogWarning("[OnSend] 알 수 없는 프로토콜");
                break;
        }
    }

    public Player GetMyPlayer()
    {
        return players[myPlayerId];
    }
    public Player GetPlayerFromId(int _id)
    {
        return players[_id];
    }
#endregion
}