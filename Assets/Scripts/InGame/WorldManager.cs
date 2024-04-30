using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Protocol;
using Reader;
using Unity.VisualScripting.Dependencies.Sqlite;
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
    private NetworkStream stream = null;
    private GameObject playerPrefab;
    private SessionInfo sessionInfo;
    private Dictionary<int, Player> players;
    private int myPlayerId;
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
        ServerManager.Instance().SetIsConnect(false);
        if (stream != null)
            stream.Close();
        if (ServerManager.Instance().GetTcpClient() != null)
            ServerManager.Instance().CloseTcpClient();
    }
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        stream = ServerManager.Instance().GetStream();
        players = new Dictionary<int, Player>();
        sessionInfo = new SessionInfo();
        InitializeGame();
        GameManager.InGame += SendGameSyncEvent;
    }
    private void Update()
    {
        if (stream.DataAvailable)
            OnReceive();
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
    private void OnReceive()
    {
        byte[] sizeData = new byte[4];
        stream.Read(sizeData, 0, sizeData.Length);
        ByteReader br = new ByteReader(sizeData);

        int jsonSize = br.ReadInt();
        byte[] data = new byte[jsonSize];
        int size = stream.Read(data, 0, data.Length);

        Debug.LogFormat("[OnReceive] 데이터 크기 : {0}", size);
        
        if (size == 0)
        {
            Debug.Log("[OnReceive] 빈 데이터가 브로드캐스킹 되었습니다.");
            return;
        }
        Message msg = DataParser.ReadJsonData<Message>(data);
        // if (msg == null)
        //     return;
        // if (ServerManager.Instance().IsHost() == true) // TODO: 내가 보낸 데이터면 이라는 조건 추가
        //     return;
        // if (players == null)
        //     return;
        Debug.LogFormat("[OnReceive] 받은 데이터 타입 : {0}", msg.type);
        switch(msg.type)
        {
            case Protocol.Type.Key:
                KeyMessage keyMessage = DataParser.ReadJsonData<KeyMessage>(data);
                ReceiveKeyEvent(keyMessage);
                break;
            case Protocol.Type.PlayerMove:
                PlayerMoveMessage moveMessage = DataParser.ReadJsonData<PlayerMoveMessage>(data);
                ReceivePlayerMoveEvent(moveMessage);
                break;

            // case Protocol.Type.PlayerReconnect:
            //     break;

            // case Protocol.Type.PlayerDisconnect:
            //     ReceivePlayerDisconnectEvent(br);
            //     break;

            // case Protocol.Type.OtherPlayerConnect:
            //     ReceiveOtherPlayerConnectEvent(br);
            //     break;

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

            // case Protocol.Type.GameStartCountDown:
            //     ReceiveGameStartCountDownEvent(br);
            //     break;

            // case Protocol.Type.GameStart:
            //     ReceiveGameStartEvent(br);
            //     break;

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
        int keyData = keyMessage.keyData;
        int id = keyMessage.playerId;

        Vector3 moveVector = Vector3.zero;
        Vector3 playerPos = players[id].GetPosition();
        if ((keyData & KeyEventCode.MOVE) == KeyEventCode.MOVE)
        {
            moveVector = new Vector3(keyMessage.x, keyMessage.y, keyMessage.z);
            moveVector = Vector3.Normalize(moveVector);
            isMove = true;
        }

        if (isMove)
        {
            players[id].SetMoveVector(moveVector);
            PlayerMoveMessage msg = new PlayerMoveMessage(id, playerPos, moveVector);
            ServerManager.Instance().SendDataToInGame<PlayerMoveMessage>(msg);
        }
    }
    // 플레이어 이동 이벤트 처리
    private void ReceivePlayerMoveEvent(PlayerMoveMessage msg)
    {
        if (ServerManager.Instance().IsHost() == true)
            return;
        Vector3 moveVecotr = new Vector3(msg.xDir, msg.yDir, msg.zDir);
        // moveVector가 같으면 방향 & 이동량 같으므로 적용 굳이 안함
        if (!moveVecotr.Equals(players[msg.playerId].moveVector))
        {
            players[msg.playerId].SetPosition(msg.xPos, msg.yPos, msg.zPos);
            players[msg.playerId].SetMoveVector(moveVecotr);
        }
    }
    // 다른 플레이어 접속 이벤트 처리
    private void ReceiveOtherPlayerConnectEvent(ByteReader br)
    {
        int newId = br.ReadInt();
        GameObject newInstance = Instantiate(playerPrefab, startingPoints[sessionInfo.totalPlayerCount].transform.position, Quaternion.identity, playerPool.transform);
        players.Add(newId, newInstance.GetComponent<Player>());
        sessionInfo.totalPlayerCount++;
    }
    // 게임 씬 로드 이벤트 처리
    private void ReceiveLoadGameSceneEvent(LoadGameSceneMessage msg)
    {
        myPlayerId = msg.playerId;
        int totalPlayerCount = msg.userCount;
        sessionInfo.totalPlayerCount = totalPlayerCount + 1;
        InputManager.instance.playerId = myPlayerId;
        List<int> userList = msg.userList;

        GameObject myPlayer = Instantiate(playerPrefab, startingPoints[totalPlayerCount].transform.position, startingPoints[0].transform.rotation, playerPool.transform);
        myPlayer.GetComponent<Player>().Initialize(true, myPlayerId, "Player" + myPlayerId);
        players.Add(myPlayerId, myPlayer.GetComponent<Player>());
        Debug.LogFormat("[WorldManager] 내 플레이어 생성 완료 : {0}", myPlayerId);
        for (int i = 0; i < totalPlayerCount; i++)
        {
            int userIndex = userList[i];
            GameObject otherPlayer = Instantiate(playerPrefab, startingPoints[i].transform.position, Quaternion.identity, playerPool.transform);
            otherPlayer.GetComponent<Player>().Initialize(false, userIndex, "Player" + userIndex);
            players.Add(userIndex, otherPlayer.GetComponent<Player>());
        }
    }
    // 게임 시작 카운트 다운 이벤트 처리
    private void ReceiveGameStartCountDownEvent(ByteReader br)
    {
        int count = br.ReadInt();
        Debug.LogFormat("[OnReceive] GameStartCountDownEvent : {0}", count);
    }
    // 게임 시작 이벤트 처리
    private void ReceiveGameStartEvent(ByteReader br)
    {
        Debug.LogFormat("[OnReceive] ReceiveGameStartEvent");
        GameManager.Instance().ChangeState(GameManager.GameState.InGame);
    }
    // 다른 플레이어 접속 끊김 이벤트 처리
    private void ReceivePlayerDisconnectEvent(ByteReader br)
    {
        int userId = br.ReadInt();
        Destroy(players[userId].gameObject);
        players.Remove(userId);
    }
    // 플레이어 위치 및 회전 동기화 이벤트 처리
    private void ReceiveGameSyncEvent(GameSyncMessage msg)
    {
        int id = 0;
        if (players == null)
            return;

        foreach (var player in players)
        {
            var y = player.Value.GetPosition().y;
            player.Value.SetPosition(new Vector3(msg.xPos[id], y, msg.zPos[id]));
            id++;
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
    // 내 위치와 회전을 서버에 보내서 모든 플레이어에게 동기화
    private void SendGameSyncEvent()
    {
        int numOfClient = players.Count;

        float[] xPos = new float[numOfClient];
        float[] zPos = new float[numOfClient];
        bool[] online = new bool[numOfClient];
        int index = 0;
        foreach (var player in players)
        {
            xPos[index] = player.Value.GetPosition().x;
            zPos[index] = player.Value.GetPosition().z;
            index++;
        }
        GameSyncMessage msg = new GameSyncMessage(myPlayerId, numOfClient, xPos, zPos, online);
        ServerManager.Instance().SendDataToInGame(msg);
    }
    // 게임 시작 이벤트를 서버에 알림
    private void SendGameStartEvent()
    {
        if (isGameStart)
            return;
        isGameStart = true;
        Debug.LogFormat("[OnSend] SendGameStartEvent");
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteByte((byte)Protocol.Type.GameStart);
        ServerManager.Instance().SendDataToInGame(data);
    }
    // 내 플레이어가 골인했음을 서버에 알림
    private void SendPlayerGoalEvent()
    {
        Debug.LogFormat("[OnSend] SendPlayerGoalEvent");
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteByte((byte)Protocol.Type.PlayerGoal);
        bw.WriteInt(myPlayerId);
        ServerManager.Instance().SendDataToInGame(data);
    }
    // 임시 서버 리셋
    private void SendResetServerEvent()
    {
        Debug.LogFormat("[OnSend] SendResetServerEvent");
        byte[] data = new byte[1024];
        ByteWriter bw = new ByteWriter(data);
        bw.WriteByte((byte)Protocol.Type.ResetServer);
        ServerManager.Instance().SendDataToInGame(data);
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

            case Protocol.Type.PlayerDisconnect:
                break;

            case Protocol.Type.OtherPlayerConnect:
                break;
                
            case Protocol.Type.OtherPlayerReconnect:
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